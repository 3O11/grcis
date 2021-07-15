using OpenTK;
using System;
using System.IO;
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
      param = "xfreq=5.0,yfreq=2.0,zfreq=3.0,xoffset=0.0,sampleCount=200";
      tooltip = "{x,y,z}freq - These parameters control how fast the function oscillates with respect to their axes. (Any valid float number)\n" +
        "{x,y,z}offset - These parameters control the offset of the function.\n" +
        "sampleCount - Sets how many points on the curve should be sampled. This option directly affects how many trignales are rendered.\n";
    }

    #endregion

    #region Instance data

    // Instance data.
    float freqX = 5.0f;
    float freqY = 2.0f;
    float freqZ = 3.0f;
    float offsetX = 0.0f;
    float offsetY = 0.0f;
    float offsetZ = 0.0f;
    int sampleCount = 10;

    #endregion

    #region Construction

    public Construction ()
    {
      // Any one-time initialization code goes here..
    }

    private void parseParams (string param)
    {
      Dictionary<string, string> splitParams = Util.ParseKeyValueList(param);
      Util.TryParse(splitParams, "xfreq", ref freqX);
      Util.TryParse(splitParams, "yfreq", ref freqY);
      Util.TryParse(splitParams, "zfreq", ref freqZ);
      Util.TryParse(splitParams, "xoffset", ref offsetX);
      Util.TryParse(splitParams, "yoffset", ref offsetX);
      Util.TryParse(splitParams, "zoffset", ref offsetX);
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

      int pointsPerCircle = 16;

      Lissajous lissajous = new Lissajous(freqX, freqY, freqZ, offsetX, offsetY, offsetZ);
      double period = lissajous.GetPeriod();
      int offset = 0;

      for(int i = 0; i < sampleCount; ++i)
      {
        Circle vertices = new Circle(pointsPerCircle);
        vertices.Scale(0.1);

        double position = (period * i) / sampleCount;

        Vector funcPos = lissajous.SampleFunc(position);
        //Vector funcDerivative = lissajous.SampleDerivative(position);
        Vector funcDerivative = Vector.Difference(lissajous.SampleFunc((period * (i - 1 % sampleCount)) / sampleCount), lissajous.SampleFunc((period * (i + 1 % sampleCount)) / sampleCount));

        vertices.Rotate(funcDerivative);
        Circle normals = new Circle(vertices);
        vertices.Translate(funcPos);

        for (int j = 0; j < pointsPerCircle; ++j)
        {
          scene.AddVertex(vertices.Points[j].GetVec3());
          scene.SetNormal(j + offset, normals.Points[j].GetVec3());
        }
        for(int j = 1; j <= pointsPerCircle && offset != 0; ++j)
        {
          //scene.AddLine((j % pointsPerCircle) + offset, ((j - 1) % pointsPerCircle) + offset);

          // This is very ugly
          scene.AddTriangle((j % pointsPerCircle) + offset, ((j - 1) % pointsPerCircle) + offset, (j % pointsPerCircle) + (offset - pointsPerCircle));
          scene.AddTriangle(((j - 1) % pointsPerCircle) + offset, ((j - 1) % pointsPerCircle) + (offset - pointsPerCircle), (j % pointsPerCircle) + (offset - pointsPerCircle));
        }

        offset += pointsPerCircle;
      }

      offset -= pointsPerCircle;
      for(int j = 1; j <= pointsPerCircle; ++j)
      {
        scene.AddTriangle((j % pointsPerCircle) + offset, ((j - 1) % pointsPerCircle) + offset, (j % pointsPerCircle));
        scene.AddTriangle(((j - 1) % pointsPerCircle) + offset, ((j - 1) % pointsPerCircle), (j % pointsPerCircle));
      }

      return 0;
    }

    #endregion

    #region Helper classes/funcs

    class Lissajous
    {
      public Lissajous (double xfreq, double yfreq, double zfreq, double xoffset, double yoffset, double zoffset)
      {
        this.xfreq = xfreq;
        this.yfreq = yfreq;
        this.zfreq = zfreq;
        this.xoffset = xoffset;
        this.yoffset = yoffset;
        this.zoffset = zoffset;
      }

      // Get the value of the function at the current position
      // (Find the circle position)
      public Vector SampleFunc(double position)
      {
        double x = Math.Sin(xfreq * position + xoffset);
        double y = Math.Sin(yfreq * position + yoffset);
        double z = Math.Sin(zfreq * position + zoffset);

        return new Vector(x, y, z);
      }

      // Get the value of the derivative at the cuttent position
      // (To find out how to rotate the circle at this position)
      public Vector SampleDerivative(double position)
      {
        double x = xfreq * Math.Cos(xfreq * position + xoffset);
        double y = yfreq * Math.Cos(yfreq * position + yoffset);
        double z = zfreq * Math.Cos(zfreq * position + zoffset);

        return new Vector(x, y, z);
      }

      public double GetPeriod ()
      {
        // Approximation of the function period
        return (2.0 * Math.PI / AdditionalMath.GCD(AdditionalMath.GCD(xfreq, yfreq), zfreq));
      }

      double xfreq;
      double yfreq;
      double zfreq;
      double xoffset;
      double yoffset;
      double zoffset;
    }

    class AdditionalMath
    {
      public static double GCD(double a, double b)
      {
        if (a < b)
          return GCD(b, a);

        if (Math.Abs(b) < 0.001)
          return a;

        else
          return (GCD(b, a - Math.Floor(a / b) * b));
      }
    }

    class Circle
    {
      public Circle(int pointCount)
      {
        Points = new List<Vector>();

        Vector v = new Vector(1, 0, 0);

        for (int i = 0; i < pointCount; i++)
        {
          double angle = (2 * Math.PI) * ((double)i / pointCount);

          Vector curr = new Vector(v);
          curr.Rotate(0, 0, angle);

          Points.Add(curr);
        }
      }

      public Circle(Circle c)
      {
        Points = new List<Vector>();
        foreach (var point in c.Points)
        {
          Points.Add(new Vector(point));
        }
      }

      public void Rotate(Vector rotation)
      {
        // Get circle normal
        Vector normal = Vector.Cross(Points[0], Points[Points.Count / 2]);

        // Get the axis of rotation and angle
        Vector axis = Vector.Cross(normal, rotation);
        double angle = Vector.Angle(normal, rotation);

        for (int i = 0; i < Points.Count; ++i)
        {
          Points[i] = Quaternion.Rotate(Points[i], axis, angle);
        }
      }

      public void Translate(Vector offset)
      {
        for(int i = 0; i < Points.Count; ++i)
        {
          Points[i].Add(offset);
        }
      }

      public void Scale(double scale)
      {
        for (int i = 0; i < Points.Count; ++i)
        {
          Points[i].Scale(scale);
        }
      }

      public List<Vector> Points { get; private set; }
    }

    class Vector
    {
      public Vector(double x, double y, double z)
      {
        this.x = x;
        this.y = y;
        this.z = z;
      }

      public Vector(Vector v)
      {
        x = v.x;
        y = v.y;
        z = v.z;
      }

      public Vector3 GetVec3 ()
      {
        return new Vector3((float)x, (float)y, (float)z);
      }

      public void Add(Vector v)
      {
        x += v.x;
        y += v.y;
        z += v.z;
      }

      public void Translate(Vector v)
      {
        x += v.x;
        y += v.y;
        z += v.z;
      }

      public void Scale(double scale)
      {
        x *= scale;
        y *= scale;
        z *= scale;
      }

      // Quat rotation
      public void Rotate(Vector v)
      {
        v.Normalize();

        x *= v.x;

        // Cross product(normal vector of the plane defined by the two vectors)
        // This defines the axis of rotation
        Vector axis = Cross(this, v);

        // Angle between two vectors
        double angle = Angle(this, v);

        Vector temp = Quaternion.Rotate(this, axis, angle);
        x = temp.x;
        y = temp.y;
        z = temp.z;
      }

      // Euler rotation
      public void Rotate(double xAngle, double yAngle, double zAngle)
      {
        if (xAngle > double.Epsilon) _xRotate(xAngle);
        if (yAngle > double.Epsilon) _yRotate(yAngle);
        if (zAngle > double.Epsilon) _zRotate(zAngle);
      }

      public double Norm ()
      {
        return Math.Sqrt(Dot(this, this));
      }

      public void Normalize ()
      {
        double norm = Norm();

        x /= norm;
        y /= norm;
        z /= norm;
      }

      void _xRotate(double angle)
      {
        double ty = y, tz = z;

        // x = x;
        this.y = ty * Math.Cos(angle) - tz * Math.Sin(angle);
        z = ty * Math.Sin(angle) + tz * Math.Cos(angle);
      }

      void _yRotate (double angle)
      {
        double tx = x, tz = z;

        x = tx * Math.Cos(angle) + tz * Math.Sin(angle);
        // y = y;
        z = -tx * Math.Sin(angle) + tz * Math.Cos(angle);
      }

      void _zRotate (double angle)
      {
        double tx = x, ty = y;

        x = tx * Math.Cos(angle) - ty * Math.Sin(angle);
        y = tx * Math.Sin(angle) + ty * Math.Cos(angle);
        //z = z;
      }

      public static Vector Difference(Vector u, Vector v)
      {
        double dx = Math.Abs(u.x - v.x);
        double dy = Math.Abs(u.y - v.y);
        double dz = Math.Abs(u.z - v.z);
        
        return new Vector(dx, dy, dz);
      }

      public static Vector Cross(Vector u, Vector v)
      {
        double cx = u.y * v.z - u.z * v.y;
        double cy = -(u.x * v.z - u.z * v.x);
        double cz = u.x * v.y - u.y * v.x;

        return new Vector(cx, cy, cz);
      }

      public static double Dot(Vector u, Vector v)
      {
        return (u.x * v.x) + (u.y * v.y) + (u.z * v.z);
      }

      public static double Angle(Vector u, Vector v)
      {
        return Math.Acos(Dot(u, v) / (u.Norm() * v.Norm()));
      }

      public double x { get; set; }
      public double y { get; set; }
      public double z { get; set; }
    }

    struct Quaternion
    {
      public Quaternion(double w, double i, double j, double k)
      {
        this.w = w;
        this.i = i;
        this.j = j;
        this.k = k;
      }

      public Quaternion(Vector axis, double angle)
      {
        // Just to be sure
        axis.Normalize();

        w = Math.Cos(angle);

        double tempSine = Math.Sin(angle);
        i = tempSine * axis.x;
        j = tempSine * axis.y;
        k = tempSine * axis.z;
      }

      public Quaternion(Vector v)
      {
        w = 0;
        i = v.x;
        j = v.y;
        k = v.z;
      }

      public static Quaternion Multiply(Quaternion p, Quaternion q)
      {
        // Hamiltonian product
        double mw = (p.w * q.w) - (p.i * q.i) - (p.j * q.j) - (p.k * q.k);
        double mi = (p.w * q.i) + (p.i * q.w) + (p.j * q.k) - (p.k * q.j);
        double mj = (p.w * q.j) - (p.i * q.k) + (p.j * q.w) + (p.k * q.i);
        double mk = (p.w * q.k) + (p.i * q.j) - (p.j * q.i) + (p.k * q.w);

        return new Quaternion(mw, mi, mj, mk);
      }

      public static Vector Rotate(Vector v, Vector axis, double angle)
      {
        Quaternion q = new Quaternion(axis, angle/2);
        Quaternion inv_q = new Quaternion(axis, -(angle/2));

        Quaternion vec = new Quaternion(v);

        vec = Multiply(vec, q);
        vec = Multiply(inv_q, vec);
        return new Vector(vec.i, vec.j, vec.k);
      }

      public double w { get; private set; }
      public double i { get; private set; }
      public double j { get; private set; }
      public double k { get; private set; }
    }

    #endregion
  }
}
