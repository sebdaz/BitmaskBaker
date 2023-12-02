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
}

[JsonSerializable(typeof(ObjectReflectionSerializer))]
public sealed class IconMeta
{
    [DataField("Name")]
    public string name = string.Empty;
    [DataField("SizeX")]
    public int sizeX = 0;
    [DataField("SizeY")]
    public int sizeY = 0;
    [DataField("PixelsPerUnit")]
    public float pixelsPerUnit = 0.0f;
    [DataField("PivotX")]
    public float pivotX = 0.0f;
    [DataField("PivotY")]
    public float pivotY = 0.0f;
    [DataField("IconStates", typeof(ListSerializer<IconStateMeta, ObjectSerializer>))]
    public List<IconStateMeta> iconStates = new List<IconStateMeta>();
}
