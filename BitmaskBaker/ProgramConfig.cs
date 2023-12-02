using AzSharp.Json.Serialization.Attributes;
using AzSharp.Json.Serialization.TypeSerializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskBaker;

[JsonSerializable(typeof(ObjectReflectionSerializer))]
internal class ProgramConfig
{
    [DataField("Width")]
    public int width = 32;
    [DataField("Height")]
    public int height = 32;
    [DataField("CutX")]
    public int cutX = 16;
    [DataField("CutY")]
    public int cutY = 16;
    [DataField("Columns")]
    public int colums = 8;
    [DataField("PivotX")]
    public float pivotX = 0.5f;
    [DataField("PivotY")]
    public float pivotY = 0.5f;
    [DataField("PixelsPerUnit")]
    public float pixelsPerUnit = 32.0f;
}
