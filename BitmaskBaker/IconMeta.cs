using AzSharp.Json.Serialization.Attributes;
using AzSharp.Json.Serialization.TypeSerializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskBaker;

[JsonSerializable(typeof(ObjectReflectionSerializer))]
public sealed class IconFrameMeta
{
    [DataField("Index")]
    public int index = 0;
    [DataField("Time")]
    public float time = 0.0f;
    public IconFrameMeta(int index, float time)
    {
        this.index = index;
        this.time = time;
    }
}

[JsonSerializable(typeof(ObjectReflectionSerializer))]
public sealed class IconStateMeta
{
    [DataField("Name")]
    public string name = string.Empty;
    [DataField("Loop")]
    public bool loop = true;
    [DataField("Reverse")]
    public bool reverse = false;
    [DataField("Frames", typeof(ListSerializer<IconFrameMeta, ObjectSerializer>))]
    public List<IconFrameMeta> frames = new List<IconFrameMeta>();
    public IconStateMeta(string name, bool loop, bool reverse, List<IconFrameMeta> frames)
    {
        this.name = name;
        this.loop = loop;
        this.reverse = reverse;
        this.frames = frames;
    }
}

[JsonSerializable(typeof(ObjectReflectionSerializer))]
public sealed class IconMeta
{
    [DataField("Name")]
    public string name = string.Empty;
    [DataField("SizeX")]
    public int sizeX = 32;
    [DataField("SizeY")]
    public int sizeY = 32;
    [DataField("PixelsPerUnit")]
    public float pixelsPerUnit = 32.0f;
    [DataField("PivotX")]
    public float pivotX = 0.5f;
    [DataField("PivotY")]
    public float pivotY = 0.5f;
    [DataField("IconStates", typeof(ListSerializer<IconStateMeta, ObjectSerializer>))]
    public List<IconStateMeta> iconStates = new List<IconStateMeta>();
    public IconMeta(string name, int sizeX, int sizeY, float pixelsPerUnit, float pivotX, float pivotY, List<IconStateMeta> iconStates)
    {
        this.name = name;
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        this.pixelsPerUnit = pixelsPerUnit;
        this.pivotX = pivotX;
        this.pivotY = pivotY;
        this.iconStates = iconStates;
    }
}
