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
      param = "none";
      tooltip = "No parameters implemented yet.";
    }

    #endregion

    #region Instance data

    // Instance data.
    private Lissajous lissajous;

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

      return 0;
    }

    class Lissajous
    {
      public Lissajous()
      {

      }

      public void Construct (SceneBrep scene, Matrix4 m)
      {

      }
    }

    #endregion
  }
}
