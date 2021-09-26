//////////////////////////////////////////////////
// Externals.

using JosefPelikan;

//////////////////////////////////////////////////
// Rendering params.

Debug.Assert(scene != null);
Debug.Assert(context != null);

// Override image resolution and supersampling.
context[PropertyName.CTX_WIDTH]         =  1920;    // whatever is convenient for your debugging/testing/final rendering
context[PropertyName.CTX_HEIGHT]        =  1080;
context[PropertyName.CTX_SUPERSAMPLING] =    16;

double end = 30.0;
context[PropertyName.CTX_START_ANIM]    =  0.0;
context[PropertyName.CTX_END_ANIM]      =  end;
context[PropertyName.CTX_FPS]           = 30.0;

//////////////////////////////////////////////////
// Preprocessing stage support.

// Uncomment the block if you need preprocessing.
/*
if (Util.TryParseBool(context, PropertyName.CTX_PREPROCESSING))
{
  double time = 0.0;
  bool single = Util.TryParse(context, PropertyName.CTX_TIME, ref time);
  // if (single) simulate only for a single frame with the given 'time'

  // TODO: put your preprocessing code here!
  //
  // It will be run only this time.
  // Store preprocessing results to arbitrary (non-reserved) context item,
  //  subsequent script calls will find it there...

  return;
}
*/

// Optional override of rendering algorithm and/or renderer.

//context[PropertyName.CTX_ALGORITHM] = new RayTracing();

int ss = 0;
if (Util.TryParse(context, PropertyName.CTX_SUPERSAMPLING, ref ss) &&
    ss > 1)
  context[PropertyName.CTX_SYNTHESIZER] = new SupersamplingImageSynthesizer
  {
    Supersampling = ss,
    Jittering = 1.0
  };

// Tooltip (if script uses values from 'param').
context[PropertyName.CTX_TOOLTIP] = "n=<double> (index of refraction)";

//////////////////////////////////////////////////
// CSG scene.

AnimatedCSGInnerNode root = new AnimatedCSGInnerNode(SetOperation.Union);
root.SetAttribute(PropertyName.REFLECTANCE_MODEL, new PhongModel());
root.SetAttribute(PropertyName.MATERIAL, new PhongMaterial(new double[] {1.0, 0.8, 0.1}, 0.1, 0.6, 0.4, 128));
scene.Intersectable = root;

// Optional Animator.
string namePos   = "partPos";
string nameColor = "colPos";

CatmullRomAnimator pa = new CatmullRomAnimator()
{
  Start =  0.0,
  End   =  end
};
pa.newProperty(namePos, 0.0, end, 8.0,
               PropertyAnimator.InterpolationStyle.Cyclic,
               new List<Vector4d[]>()
               {
                 new Vector4d[]
                 {
                   new Vector4d(0.0, 0.2,-0.2, 0.4),
                   new Vector4d(2.0, 0.2,-0.2, 0.2),
                   new Vector4d(2.0, 1.2, 0.0, 1.5),
                   new Vector4d(0.0, 2.2, 0.0, 1.8),
                 },
                 new Vector4d[]
                 {
                   new Vector4d(2.0, 0.2,-0.2, 0.2),
                   new Vector4d(2.0, 1.2, 0.0, 1.5),
                   new Vector4d(0.0, 2.2, 0.0, 1.8),
                   new Vector4d(0.0, 0.2,-0.2, 0.4),
                 },
                 new Vector4d[]
                 {
                   new Vector4d(2.0, 1.2, 0.0, 1.5),
                   new Vector4d(0.0, 2.2, 0.0, 1.8),
                   new Vector4d(0.0, 0.2,-0.2, 0.4),
                   new Vector4d(2.0, 0.2,-0.2, 0.2),
                 },
                 new Vector4d[]
                 {
                   new Vector4d(0.0, 2.2, 0.0, 1.8),
                   new Vector4d(0.0, 0.2,-0.2, 0.4),
                   new Vector4d(2.0, 0.2,-0.2, 0.2),
                   new Vector4d(2.0, 1.2, 0.0, 1.5),
                 },
               },
               true);
pa.newProperty(nameColor, 0.0, end, 6.0,
               PropertyAnimator.InterpolationStyle.Cyclic,
               new List<Vector3[]>()
               {
                 new Vector3[]
                 {
                   new Vector3(1.0f, 0.3f, 0.2f),
                   new Vector3(0.5f, 0.9f, 0.5f),
                   new Vector3(0.2f, 0.2f, 1.0f),
                   new Vector3(0.0f, 0.8f, 1.0f),
                 },
                 new Vector3[]
                 {
                   new Vector3(0.3f, 0.9f, 0.2f),
                   new Vector3(0.0f, 0.3f, 0.7f),
                   new Vector3(0.4f, 0.4f, 0.1f),
                   new Vector3(0.3f, 0.7f, 0.6f),
                 },
                 new Vector3[]
                 {
                   new Vector3(0.5f, 0.5f, 0.8f),
                   new Vector3(0.7f, 0.4f, 0.6f),
                   new Vector3(0.2f, 0.6f, 0.8f),
                   new Vector3(0.2f, 0.4f, 1.0f),
                 },
               },
               true);
scene.Animator = pa;

// Background color.
scene.BackgroundColor = new double[] {0.0, 0.01, 0.03};
scene.Background = new StarBackground(scene.BackgroundColor, 600, 0.004); // 1000, 0.002

// Camera.
AnimatedCamera cam = new AnimatedCamera(new Vector3d(0.7, -0.4,  0.0),
                                        new Vector3d(0.7,  0.8, -6.0),
                                        70.0);
cam.End = 10.0; // one complete turn takes 10.0 seconds
if (scene is ITimeDependent ascene)
  ascene.End = 30.0;
scene.Camera  = cam;

// Light sources:
scene.Sources = new LinkedList<ILightSource>();
scene.Sources.Add(new AmbientLightSource(0.8));
scene.Sources.Add(new PointLightSource(new Vector3d(-5.0, 4.0, -3.0), 1.2));

// --- NODE DEFINITIONS ----------------------------------------------------

// Params dictionary.
Dictionary<string, string> p = Util.ParseKeyValueList(param);

// n = <index-of-refraction>
double n = 1.6;
Util.TryParse(p, "n", ref n);

// Transparent sphere.
Sphere sphere;
sphere = new Sphere();
PhongMaterial pm = new PhongMaterial(new double[] {0.0, 0.2, 0.1}, 0.03, 0.03, 0.08, 128);
pm.n  = n;
pm.Kt = 0.9;
sphere.SetAttribute(PropertyName.MATERIAL, pm);
root.InsertChild(sphere, Matrix4d.Identity);

// Opaque sphere.
sphere = new Sphere();
pm = new PhongMaterial(new double[] {0.5, 0.0, 1.0}, 0.03, 0.03, 0.08, 128);
sphere.SetAttribute(PropertyName.MATERIAL, pm);
root.InsertChild(sphere, Matrix4d.Scale(1.2) * Matrix4d.CreateTranslation(1.5, 0.2, 2.4));

// Cut out cube
Cube cube = new Cube();
pm = new PhongMaterial(new double[] {0.0, 0.0, 0.1}, 0.03, 0.03, 0.08, 128);
cube.SetAttribute(PropertyName.MATERIAL, pm);
CSGInnerNode difference = new CSGInnerNode(SetOperation.Difference);
difference.InsertChild(cube, Matrix4d.Scale(1));
Cube differenceCube = new Cube();
pm = new PhongMaterial(new double[] {0.0, 0.0, 1.0}, 0.03, 0.03, 0.08, 128);
differenceCube.SetAttribute(PropertyName.MATERIAL, pm);
difference.InsertChild(differenceCube, Matrix4d.Scale(1) * Matrix4d.CreateTranslation(-0.5, 0.0, -0.5));
root.InsertChild(difference, Matrix4d.Scale(3) * Matrix4d.CreateTranslation(5, 0, 10));

Cylinder cylinder = new Cylinder();


BezierSurface bezierSurface = new BezierSurface(1, 2, new double[] {
0.0, 0.0, 3.0,  // row 0
1.0, 0.0, 3.0,
2.0, 0.0, 3.0,
3.0, 0.0, 3.0,
4.0, 0.0, 3.0,
5.0, 0.0, 3.0,
6.0, 0.0, 3.0,
0.0, 0.0, 2.0,  // row 1
1.0, 0.0, 2.0,
2.0, 3.0, 2.0,
3.0, 3.0, 2.0,
4.0, 3.0, 2.0,
5.0, 0.0, 2.0,
6.0, 0.0, 2.0,
0.0, 0.0, 1.0,  // row 2
1.0, 0.0, 1.0,
2.0, 0.0, 1.0,
3.0, 1.5, 1.0,
4.0, 3.0, 1.0,
5.0, 0.0, 1.0,
6.0, 0.0, 1.0,
0.0, 0.0, 0.0,  // row 3
1.0, 0.0, 0.0,
2.0, 0.0, 0.0,
3.0, 0.0, 0.0,
4.0, 0.0, 0.0,
5.0, 0.0, 0.0,
6.0, 0.0, 0.0,
});
bezierSurface.PreciseNormals = true;
bezierSurface.SetAttribute(PropertyName.COLOR, new double[] {1.0, 1.0, 1.0});
bezierSurface.SetAttribute(PropertyName.TEXTURE, new CheckerTexture(10.5, 12.0, new double[] {0.2, 0.0, 0.1}));
root.InsertChild(bezierSurface, Matrix4d.Scale(1) * Matrix4d.CreateTranslation(-5, 0, -15));

RecursionFunction del = (Intersection i, Vector3d dir, double importance, out RayRecursion rr) =>
{
  double direct = 1.0 - i.TextureCoord.X;
  direct = Math.Pow(direct * direct, 6.0);

  rr = new RayRecursion(
    Util.ColorClone(i.SurfaceColor, direct),
    new RayRecursion.RayContribution(i, dir, importance));

  return 144L;
};

// Particle object.
ISolid particles = new ChaoticParticles(
  new Vector4d[]
  {
    new Vector4d( 0.0, 0.2,-0.2, 0.4),
    new Vector4d( 2.0, 0.2,-0.2, 0.2),
    new Vector4d( 2.0, 1.2, 0.0, 1.5),
    new Vector4d( 0.0, 2.2, 0.0, 1.8),
    new Vector4d( 0.0, 3.2, 1.0, 2.8),
    new Vector4d( 4.0, 2.2, 0.0, 0.3),
    new Vector4d( 0.0, 2.2, 1.0, 0.9),
    new Vector4d(-0.5, 2.2, 0.0, 1.8),
  },
  new Vector3[]
  {
    new Vector3(1.0f, 0.3f, 0.2f),
    new Vector3(0.5f, 0.9f, 0.5f),
    new Vector3(0.2f, 0.2f, 1.0f),
    new Vector3(0.0f, 0.8f, 1.0f),
    new Vector3(1.0f, 0.8f, 1.0f),
    new Vector3(1.0f, 0.8f, 0.0f),
    new Vector3(0.4f, 0.8f, 1.0f),
    new Vector3(0.5f, 0.1f, 0.3f),
  },
  namePos, nameColor);
particles.SetAttribute(PropertyName.RECURSION, del);
particles.SetAttribute(PropertyName.NO_SHADOW, true);
particles.SetAttribute(PropertyName.COLOR, new double[] {0.3, 0.9, 1.0});
root.InsertChild(particles, Matrix4d.CreateTranslation(5.0, 0.0, -5.0));

// Infinite plane with checker texture.
Plane plane = new Plane();
plane.SetAttribute(PropertyName.COLOR, new double[] {0.2, 0.03, 0.0});
plane.SetAttribute(PropertyName.TEXTURE, new CheckerTexture(0.6, 0.6, new double[] {1.0, 1.0, 1.0}));
root.InsertChild(plane, Matrix4d.RotateX(-MathHelper.PiOver2) * Matrix4d.CreateTranslation(0.0, -1.0, 0.0));
