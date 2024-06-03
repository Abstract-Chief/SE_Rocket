public class Camera
{
    public IMyCameraBlock camera;
    public MyDetectedEntityInfo last_scan;
    bool last_flag;
    long time_scan;

    public Camera(IMyCameraBlock camera_)
    {
        camera = camera_;
        camera.EnableRaycast = true;
        last_flag = false;
        time_scan = -1;
    }


    public string GetRayCastInfo()
    {
        string result = "";
        result += "Entity Id: " + last_scan.EntityId;
        result += "\nName: " + last_scan.Name;
        result += "\nType: " + last_scan.Type;
        result += "\nVelocity: " + last_scan.Velocity.ToString("0.000");
        result += "\nRelationship: " + last_scan.Relationship;
        result += "\nPosition: " + last_scan.HitPosition.ToString() + "\n";
        return result;
    }
    int OST_SCAN = 100;
    public int Scan(Vector3D coord)
    {
        time_scan = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        double distance = (coord - camera.GetPosition()).Length() + OST_SCAN;
        if (camera.AvailableScanRange < distance) return 1;
        MyDetectedEntityInfo rayinfo = camera.Raycast(coord + (Vector3D.Normalize(coord - camera.GetPosition()) * OST_SCAN));
        if ((rayinfo.Type != MyDetectedEntityType.SmallGrid && rayinfo.Type != MyDetectedEntityType.LargeGrid) || rayinfo.EntityId == camera.EntityId) return 2;
        last_scan = rayinfo;
        last_flag = true;
        if (last_flag) return 0;
        return 0;
    }
}
class Radar
{
    int p_l;
    float p_s;
    public List<Camera> cameras;
    public MyDetectedEntityInfo last_detect;
    public string name;
    public bool use;
    long last_detect_t;
    long get_time()
    {
        if (last_detect_t == -1) return -1;
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - last_detect_t;

    }
    long get_pilings(double distance)
    {
        return (long)(1000 / (cameras.Count * 2) / (p_l * distance));
    }
    public Radar(List<Camera> cameras_ , string name_, int points_level, float point_size)
    {
        p_l = points_level;
        p_s = point_size;
        cameras = cameras_;
        name = name_;
        last_detect_t = -1;
        use = false;
    }
    List<Vector3D> get_points(Matrix matrix, Vector3D coord)
    {
        List<Vector3D> r = new List<Vector3D>();
        if (p_l == 1) r.Add(coord);
        if (p_l > 1)
        {
            for (int i = 1; i < p_l; i++)
            {
                r.Add(Vector3D.Normalize(matrix.Up) * p_s * i + coord);
                r.Add(Vector3D.Normalize(-matrix.Up) * p_s * i + coord);
                r.Add(Vector3D.Normalize(-matrix.Left) * p_s * i + coord);
                r.Add(Vector3D.Normalize(matrix.Left) * p_s * i + coord);
            }
        }
        return r;

    }
    public void FirstScan(MyDetectedEntityInfo info)
    {
        if (info.Type != MyDetectedEntityType.SmallGrid && info.Type != MyDetectedEntityType.LargeGrid) return;
        last_detect = info;
        last_detect_t = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        use = true;
    }
    public string Scan()
    {

        if (cameras.Count == 0) return "no cam";
        if (last_detect.Position == Vector3D.Zero) return "wait enemy";
        Vector3D coord_to = last_detect.Position + last_detect.Velocity * get_time() / 1000;
        if (get_time() < get_pilings((coord_to - cameras[0].camera.GetPosition()).Length()))
            return "wait piling";

        List<Vector3D> p = get_points(cameras[0].camera.WorldMatrix, coord_to);
        int i = 0;
        string outs = "";
        foreach (Camera cam in cameras)
        {
            int r = cam.Scan(p[i]);
            outs += $"{i}- {r}\n";
            if (r == 0 && cam.last_scan.EntityId==last_detect.EntityId)
            {
                last_detect = cam.last_scan;
                last_detect_t = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                break;
            }
            else if (r == 2) i++;
            if (i >= p.Count) break;
        }

        return outs + $"cool {last_detect.Name} {get_time()}";
    }

    public Vector3D GetPosition()
    {
        return cameras[0].camera.GetPosition();
    }
}
 IMyProgrammableBlock aim;
Radar radar;
public Program()
{
aim = GridTerminalSystem.GetBlockWithName("RocketAIM") as IMyProgrammableBlock;
    IMyBlockGroup g = GridTerminalSystem.GetBlockGroupWithName("radar_cam");
    List<IMyCameraBlock> cams = new List<IMyCameraBlock>();
    g.GetBlocksOfType<IMyCameraBlock>(cams);
    List<Camera> cams2 = new List<Camera>();
    foreach (IMyCameraBlock c in cams) cams2.Add(new Camera(c));
    radar = new Radar(cams2, "radar", 2,2.5f);
    

}
float max_time = 360f;
Vector3D calculate(MyDetectedEntityInfo info)
{
    Vector3D vector = info.Position - Me.GetPosition();
    for(float time = 0; time < max_time; time += 0.05f)
    {
        Vector3D r = vector / time + info.Velocity;
        if (r.Length() <= 100 && r.Length() > 93) return r * time + Me.GetPosition();
    }
    return Vector3D.Zero;
}
void Main(string args)
{
    if (args.Length>=1)
    {
        string[] data = args.Split(':');
        MyDetectedEntityInfo rayinfo;
         Vector3D v=new Vector3D(double.Parse(data[0]), double.Parse(data[1]), double.Parse(data[2]));
        foreach(Camera c in radar.cameras){
            if(c.camera.AvailableScanRange<=(v-c.camera.GetPosition()).Length()) continue;
            rayinfo= c.camera.Raycast(v);
            if(rayinfo.EntityId!=Me.EntityId && rayinfo.Position!=Vector3D.Zero){
                Echo($"{rayinfo.Position}");
                 if (rayinfo.Type == MyDetectedEntityType.SmallGrid || rayinfo.Type == MyDetectedEntityType.LargeGrid)
                  {
                    radar.FirstScan(rayinfo);
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    }
                break;
            }
        }
        
    }
    Vector3D save=radar.last_detect.Position;
    radar.Scan();
    
    Vector3D vec = calculate(radar.last_detect);
    if(vec!=Vector3D.Zero && save==radar.last_detect.Position)
        aim.CustomData = $"{vec.X}:{vec.Y}:{vec.Z}";
    
}

