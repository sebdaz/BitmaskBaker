using AzSharp.Json.Parsing;
using AzSharp.Json.Serialization;
using AzSharp.Json.Serialization.TypeSerializers;
using System;
using System.IO;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using SixLabors.ImageSharp.PixelFormats;

namespace BitmaskBaker;

public enum CornerType
{
    NW,
    NE,
    SW,
    SE
}

public enum CornerEvaluation
{
    FULL,
    CORNER,
    NORTHSOUTH,
    WESTEAST,
    NONE
}

public enum BitDirection : int
{
    NORTH = 1,
    SOUTH = 2,
    EAST = 4,
    WEST = 8,
    NORTHEAST = 16,
    SOUTHEAST = 32,
    NORTHWEST = 64,
    SOUTHWEST = 128,
    ALL_DIRECTIONS = 255
}

public sealed class CornerImageSet
{
    public CornerImage fullImage;
    public CornerImage NSImage;
    public CornerImage WEImage;
    public CornerImage cornerImage;
    public CornerImage wallImage;
    public CornerImageSet(CornerImage fullImage, CornerImage nSImage, CornerImage wEImage, CornerImage cornerImage, CornerImage wallImage)
    {
        this.fullImage = fullImage;
        this.NSImage = nSImage;
        this.WEImage = wEImage;
        this.cornerImage = cornerImage;
        this.wallImage = wallImage;
    }
}

public sealed class CornerImage
{
    public Image<Rgba32> NW;
    public Image<Rgba32> NE;
    public Image<Rgba32> SW;
    public Image<Rgba32> SE;
    public CornerImage(Image<Rgba32> nW, Image<Rgba32> nE, Image<Rgba32> sW, Image<Rgba32> sE)
    {
        NW = nW;
        NE = nE;
        SW = sW;
        SE = sE;
    }
}

internal class Program
{
    public static ProgramConfig config = new();
    static void Main(string[] args)
    {
        Console.WriteLine("Starting bitmask baking!");

        Directory.CreateDirectory("input");
        Directory.CreateDirectory("output");
        if (File.Exists(@"config.json"))
        {
            JsonNode node = new JsonNode();
            JsonError error = new JsonError();
            node.LoadFile("config.json", error);
            config = JsonSerializer.Deserialize<ProgramConfig, ObjectSerializer>(null, node)!;
            JsonNode savedNode = JsonSerializer.Serialize<ProgramConfig, ObjectSerializer>(config);
            savedNode.SaveFile("config.json");
        }
        else
        {
            JsonNode node = JsonSerializer.Serialize<ProgramConfig, ObjectSerializer>(config);
            node.SaveFile("config.json");
            Console.WriteLine("Generated config file - configure the file and re-start the app");
            Console.ReadLine();
            return;
        }

        foreach (var file in Directory.GetFiles("input"))
        {
            ProcessInputImage(file);
        }
        Console.WriteLine($"TASK DONE!!! Press any key to exit.");
        Console.ReadKey();
    }
    public static void ProcessInputImage(string imagePath)
    {
        var fileName = Path.GetFileName(imagePath);
        var bareName = Path.GetFileNameWithoutExtension(imagePath);
        Console.WriteLine($"Processing {fileName}...");
        string path = $"output/{bareName}";
        Directory.CreateDirectory(path);

        Image<Rgba32> image = Image.Load<Rgba32>(imagePath);

        CornerImageSet cornerImages = SplitSourceImage(image);

        List<IconStateMeta> iconStates = new();
        List<Image<Rgba32>> imageList = new();

        int imageIndex = 0;
        for (int value = 0; value <= (int)BitDirection.ALL_DIRECTIONS; value++)
        {
            if (InvalidValue(value))
            {
                continue;
            }
            Image<Rgba32> NW = GetCornerImageForCorner(value, cornerImages, CornerType.NW);
            Image<Rgba32> NE = GetCornerImageForCorner(value, cornerImages, CornerType.NE);

            Image<Rgba32> SW = GetCornerImageForCorner(value, cornerImages, CornerType.SW); ;
            Image<Rgba32> SE = GetCornerImageForCorner(value, cornerImages, CornerType.SE); ;

            CornerImage compiledCorners = new CornerImage(NW, NE, SW, SE);

            Image<Rgba32> valueResult = CombineImage(compiledCorners);
            imageList.Add(valueResult);

            iconStates.Add(new IconStateMeta($"{value}", false, false, new List<IconFrameMeta>() { new IconFrameMeta(imageIndex, 1.0f) }));
            imageIndex++;
        }
        int imageAmount = imageList.Count;
        int rows = (int)Math.Ceiling((float)imageAmount / (float)config.colums);

        Image<Rgba32> finalImage = new Image<Rgba32>(config.colums * config.width, rows * config.height);
        int imagesProcessed = 0;
        for (int row = 0; row < rows; row++)
        {
            if (imagesProcessed >= imageList.Count)
            {
                break;
            }
            for (int column = 0; column < config.colums; column++)
            {
                if (imagesProcessed >= imageList.Count)
                {
                    break;
                }
                Image<Rgba32> iteratedImage = imageList[imagesProcessed];
                ImprintImage(iteratedImage, finalImage, column * config.width, row * config.height);
                imagesProcessed++;
            }
        }

        finalImage.SaveAsPng($"{path}/image.png");

        IconMeta meta = new IconMeta(bareName, config.width, config.height, config.pixelsPerUnit, config.pivotX, config.pivotY, iconStates);
        JsonNode metaNode = JsonSerializer.Serialize<IconMeta, ObjectSerializer>(meta);
        metaNode.SaveFile($"{path}/meta.json");

        Console.WriteLine($"Finished!");
    }

    public static bool InvalidValueCheck(int value, BitDirection checkComp, BitDirection requisiteOne, BitDirection requisitTwo)
    {
        if (HasBit(value, checkComp))
        {
            if (!HasBit(value, requisiteOne) || !HasBit(value, requisitTwo))
            {
                return true;
            }
        }
        return false;
    }
    public static bool InvalidValue(int value)
    {
        if (InvalidValueCheck(value, BitDirection.NORTHEAST, BitDirection.NORTH, BitDirection.EAST))
        {
            return true;
        }
        if (InvalidValueCheck(value, BitDirection.SOUTHEAST, BitDirection.SOUTH, BitDirection.EAST))
        {
            return true;
        }
        if (InvalidValueCheck(value, BitDirection.NORTHWEST, BitDirection.NORTH, BitDirection.WEST))
        {
            return true;
        }
        if (InvalidValueCheck(value, BitDirection.SOUTHWEST, BitDirection.SOUTH, BitDirection.WEST))
        {
            return true;
        }
        return false;
    }
    public static bool HasBit(int value, BitDirection bit)
    {
        if ((value & (int)bit) != 0)
        {
            return true;
        }
        return false;
    }
    public static CornerEvaluation DoCornerEvaluation(int value, BitDirection diagonal, BitDirection nsComp, BitDirection weComp)
    {
        if (HasBit(value, diagonal))
        {
            return CornerEvaluation.FULL;
        }
        else
        {
            if (HasBit(value, nsComp) && HasBit(value, weComp))
            {
                return CornerEvaluation.CORNER;
            }
            else if (HasBit(value, nsComp))
            {
                return CornerEvaluation.NORTHSOUTH;
            }
            else if (HasBit(value, weComp))
            {
                return CornerEvaluation.WESTEAST;
            }
            else
            {
                return CornerEvaluation.NONE;
            }
        }
    }
    public static Image<Rgba32> GetCornerImageForCorner(int value, CornerImageSet cornerSet, CornerType type)
    {
        CornerImage cornerImage;
        CornerEvaluation evaluation;
        switch (type)
        {
            case CornerType.NW:
                evaluation = DoCornerEvaluation(value, BitDirection.NORTHWEST, BitDirection.NORTH, BitDirection.WEST);
                break;
            case CornerType.NE:
                evaluation = DoCornerEvaluation(value, BitDirection.NORTHEAST, BitDirection.NORTH, BitDirection.EAST);
                break;
            case CornerType.SW:
                evaluation = DoCornerEvaluation(value, BitDirection.SOUTHWEST, BitDirection.SOUTH, BitDirection.WEST);
                break;
            case CornerType.SE:
                evaluation = DoCornerEvaluation(value, BitDirection.SOUTHEAST, BitDirection.SOUTH, BitDirection.EAST);
                break;
            default:
                throw new InvalidOperationException();
        }

        switch (evaluation)
        {
            case CornerEvaluation.FULL:
                cornerImage = cornerSet.fullImage;
                break;
            case CornerEvaluation.CORNER:
                cornerImage = cornerSet.cornerImage;
                break;
            case CornerEvaluation.NORTHSOUTH:
                cornerImage = cornerSet.NSImage;
                break;
            case CornerEvaluation.WESTEAST:
                cornerImage = cornerSet.WEImage;
                break;
            case CornerEvaluation.NONE:
                cornerImage = cornerSet.wallImage;
                break;
            default:
                throw new InvalidOperationException();
        }

        switch (type)
        {
            case CornerType.NW:
                return cornerImage.NW;
            case CornerType.NE:
                return cornerImage.NE;
            case CornerType.SW:
                return cornerImage.SW;
            case CornerType.SE:
                return cornerImage.SE;
            default:
                throw new InvalidOperationException();
        }
    }
    public static Image<Rgba32> CombineImage(CornerImage cornerImage)
    {
        Image<Rgba32> newImage = new Image<Rgba32>(config.width, config.height);

        int cutX = config.cutX;
        int cutY = config.cutY;

        ImprintImage(cornerImage.NW, newImage, 0, 0);
        ImprintImage(cornerImage.NE, newImage, cutX, 0);
        ImprintImage(cornerImage.SW, newImage, 0, cutY);
        ImprintImage(cornerImage.SE, newImage, cutX, cutY);

        return newImage;
    }
    public static CornerImageSet SplitSourceImage(Image<Rgba32> image)
    {
        int width = config.width;
        CornerImage fullImage = CreateSplitImage(image, 0, 0);
        CornerImage NSImage = CreateSplitImage(image, width * 1, 0);
        CornerImage WEImage = CreateSplitImage(image, width * 2, 0);
        CornerImage CornerImage = CreateSplitImage(image, width * 3, 0);
        CornerImage wallImage = CreateSplitImage(image, width * 4, 0);

        return new CornerImageSet(fullImage, NSImage, WEImage, CornerImage, wallImage);
    }
    public static CornerImage CreateSplitImage(Image<Rgba32> image, int x, int y)
    {
        int width = config.width;
        int height = config.height;
        Image<Rgba32> newImage = ImagePart(image, x, y, config.width, config.height);

        return SplitImageToCorners(newImage);
    }
    public static CornerImage SplitImageToCorners(Image<Rgba32> image)
    {
        int cutX = config.cutX;
        int cutY = config.cutY;

        int uncutX = config.width - cutX;
        int uncutY = config.height - cutY;

        Image<Rgba32> NW = ImagePart(image, 0, 0, cutX, cutY);
        Image<Rgba32> NE = ImagePart(image, cutX, 0, uncutX, cutY);

        Image<Rgba32> SW = ImagePart(image, 0, cutY, cutX, uncutY);
        Image<Rgba32> SE = ImagePart(image, cutX, cutY, uncutX, uncutY);

        return new CornerImage(NW, NE, SW, SE);

    }
    public static Image<Rgba32> ImagePart(Image<Rgba32> image, int x, int y, int width, int height)
    {
        Image<Rgba32> newImage = new Image<Rgba32>(width, height);
        for (int iy = 0; iy < height; iy++)
        {
            int yPos = y + iy;

            for (int ix = 0; ix < width; ix++)
            {
                int xPos = x + ix;

                Rgba32 color = image[xPos, yPos];
                newImage[ix, iy] = color;
            }
        }
        return newImage;
    }
    public static void ImprintImage(Image<Rgba32> imprint, Image<Rgba32> image, int xPos, int yPos)
    {
        for (int y = 0; y < imprint.Height; y++)
        {
            for (int x = 0; x < imprint.Width; x++)
            {
                int imageXPos = xPos + x;
                int imageYPos = yPos + y;

                image[imageXPos, imageYPos] = imprint[x, y];
            }
        }
    }
}
