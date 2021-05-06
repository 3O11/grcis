using OpenTK;
using System;
using System.Collections.Generic;
using Utilities;

namespace Scene3D
{
  public class Construction
  {
    #region Form initialization

    /// <summary>
    /// Optional data initialization.
    /// </summary>
    public static void InitParams (out string name, out string param, out string tooltip)
    {
      name = "Viktor Soukup";
      param = "xfreq=5.0,yfreq=2.0,zfreq=3.0,sampleCount=200";
      tooltip = "{x,y,z}freq - These parameters control how fast the function oscillates with respect to their axes. (Any valid float number)\n" +
        "sampleCount - Sets how many points on the curve should be sampled. This option directly affects how many trignales are rendered.\n";
    }

    #endregion

    #region Instance data

    // Instance data.
    private Lissajous lissajous;
    float freqX;
    float freqY;
    float freqZ;
    int sampleCount;

    #endregion

    #region Construction

    public Construction ()
    {
      // Any one-time initialization code goes here..
    }

    private void parseParams (string param)
    {
      /*
       * Zombie code that will be removed soon
       * 
      level = 2;
      radius = 0.25f;
      size = 0.25f;
      cdetail = 5;
      bdetail = 5;
      Dictionary<string, string> p = Util.ParseKeyValueList( param );
      if (p.Count > 0)
      {
        if (Util.TryParse(p, "level", ref level) && level < 1)
        {
          level = 1;
        }
        if (Util.TryParse(p, "size", ref size) && (size < 0 || size > 0.49f))
        {
          size = 0.25f;
        }
        if (Util.TryParse(p, "radius", ref radius))
        {
          if (radius < 0)
            radius = 0.25f;
          else
          {
            if (radius * 2 > size)
            {
              radius = 2 * size - 0.05f;
              if (radius < 0)
                radius = 0.05f;
            }
          }
        }
        if (Util.TryParse(p, "cdetail", ref cdetail) && cdetail < 3)
        {
          cdetail = 3;
        }
        if (Util.TryParse(p, "bdetail", ref bdetail) && bdetail < 3)
        {
          bdetail = 3;
        }
      }
      */


      Dictionary<string, string> splitParams = Util.ParseKeyValueList(param);
      Util.TryParse(splitParams, "xfreq", ref freqX);
      Util.TryParse(splitParams, "yfreq", ref freqY);
      Util.TryParse(splitParams, "zfreq", ref freqZ);
      Util.TryParse(splitParams, "sampleCount", ref sampleCount);
    }

    #endregion

    #region Mesh construction

    /// <summary>
    /// Construct a new Brep solid (preferebaly closed = regular one).
    /// </summary>
    /// <param name="scene">B-rep scene to be modified</param>
    /// <param name="m">Transform matrix (object-space to world-space)</param>
    /// <param name="param">Shape parameters if needed</param>
    /// <returns>Number of generated faces (0 in case of failure)</returns>
    public int AddMesh (SceneBrep scene, Matrix4 m, string param)
    {
      parseParams(param);
      scene.Reserve(10000000);

      lissajous = new Lissajous(freqX, freqY, freqZ, sampleCount);
      lissajous.Construct(scene, m);

      return 0;
    }

    class Lissajous
    {
      public Lissajous(float freqX, float freqY, float freqZ, int sampleCount)
      {
        this.freqX = freqX;
        this.freqY = freqY;
        this.freqZ = freqZ;
        this.amplitude = 1.0f;
        this.sampleCount = sampleCount;
      }

      public void Construct(SceneBrep scene, Matrix4 m)
      {
        
      }

      public void TrivialConstruct (SceneBrep scene, Matrix4 m)
      {

        for (int i = 0; i < sampleCount; i++)
        {
          float currentPos = (2 * (float)Math.PI * i) / sampleCount;

          float posX = (float)Math.Sin(freqX * currentPos);
          float posY = (float)Math.Sin(freqY * currentPos);
          float posZ = (float)Math.Sin(freqZ * currentPos);

          scene.AddVertex(new Vector3(posX, posY, posZ));
        }

        for (int i = 0; i < sampleCount - 1; ++i)
        {
          scene.AddLine(i, i + 1);
        }
      }

      float FindFuncPeriod()
      {

        return (float)(2 * Math.PI);
      }

      float freqX, freqY, freqZ;
      float amplitude;
      int sampleCount;
    }

    class Vector
    {
      public Vector(float x, float y, float z)
      {
        this.x = x;
        this.y = y;
        this.z = z;
      }

      public void Transform(Matrix4 m)
      {

      }

      public void RotateX(float angle)
      {

      }

      public void RotateY (float angle)
      {

      }

      public void RotateZ (float angle)
      {

      }

      public float x { private set; get; }
      public float y { private set; get; }
      public float z { private set; get; }
    }

    #endregion
  }
}
