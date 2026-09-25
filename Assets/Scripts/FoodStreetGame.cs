using System.Collections.Generic;
using UnityEngine;

// Self-contained, asset-free prototype. All world geometry is generated at runtime.
public class FoodStreetGame : MonoBehaviour
{
    class Customer { public Transform body; public float patience; }
    readonly List<Customer> customers = new List<Customer>();
    Transform player;
    Camera cam;
    Material floor, dark, pink, amber, green, white, pizza;
    float cookTimer, spawnTimer, toastUntil;
    int ready, cash, served, capacity = 2, upgradeCost = 25;
    string toast = "";
    Vector2 joystickOrigin, joystickCurrent;
    bool dragging;
    const float CookSeconds = 3.5f;

    Material Mat(Color color, float metallic = 0f)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(shader) { color = color };
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        return m;
    }
    GameObject Box(string name, Vector3 pos, Vector3 scale, Material material, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name; go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().material = material;
        if (parent) go.transform.SetParent(parent, true);
        return go;
    }
    GameObject Sphere(string name, Vector3 pos, Vector3 scale, Material material, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name; go.transform.position = pos; go.transform.localScale = scale;
        go.GetComponent<Renderer>().material = material;
        if (parent) go.transform.SetParent(parent, true);
        return go;
    }
    void Start()
    {
        Application.targetFrameRate = 60;
        cash = PlayerPrefs.GetInt("foodstreet.cash", 0);
        served = PlayerPrefs.GetInt("foodstreet.served", 0);
        capacity = PlayerPrefs.GetInt("foodstreet.capacity", 2);
        upgradeCost = PlayerPrefs.GetInt("foodstreet.upgradeCost", 25);
        floor = Mat(new Color(.12f,.16f,.22f)); dark = Mat(new Color(.08f,.09f,.14f));
        pink = Mat(new Color(1f,.24f,.51f)); amber = Mat(new Color(1f,.69f,.22f));
        green = Mat(new Color(.26f,.9f,.72f)); white = Mat(new Color(.89f,.92f,.94f));
        pizza = Mat(new Color(.95f,.43f,.13f));
        RenderSettings.ambientLight = new Color(.7f,.7f,.86f);
        Box("Restaurant floor", Vector3.zero, new Vector3(15,.2f,20), floor);
        Box("Back wall", new Vector3(0,2.2f,9.9f), new Vector3(15,4.5f,.25f), dark);
        Box("Left wall", new Vector3(-7.4f,2.2f,0), new Vector3(.25f,4.5f,20), dark);
        Box("Right wall", new Vector3(7.4f,2.2f,0), new Vector3(.25f,4.5f,20), dark);
        Box("Cook counter", new Vector3(-3.7f,.55f,2.5f), new Vector3(4,1.1f,2), pink);
        Box("Serving counter", new Vector3(3.7f,.55f,2.5f), new Vector3(4,1.1f,2), amber);
        Box("Upgrade pad", new Vector3(0,.05f,-5.3f), new Vector3(2.5,.1f,2.1f), green);
        for (int i=0;i<3;i++)
        {
            Box("Ceiling lamp", new Vector3(-5+i*5,4.3f,2), new Vector3(1.4f,.1f,1.4f), amber);
            Box("Floor detail", new Vector3(-5+i*5,.12f,-1), new Vector3(.09f,.02f,15), white);
        }
        Sphere("Pizza display", new Vector3(-3.7f,1.2f,2.5f), new Vector3(1.3f,.14f,1.3f), pizza);
        player = new GameObject("Player").transform;
        player.position = new Vector3(0,.9f,-2);
        Box("Apron", player.position, new Vector3(.8f,1.2f,.55f), white, player);
        Sphere("Head", player.position + Vector3.up*.88f, new Vector3(.68f,.68f,.68f), amber, player);
        Box("Chef hat", player.position + Vector3.up*1.35f, new Vector3(.7f,.34f,.7f), white, player);
        // Child meshes retain their world positions; move root only after creation.
        var cameraGo = new GameObject("Camera"); cam = cameraGo.AddComponent<Camera>();
        cam.transform.position = new Vector3(0,17,-14);
        cam.transform.rotation = Quaternion.Euler(48,0,0);
        cam.fieldOfView = 52; cam.backgroundColor = new Color(.12f,.11f,.2f);
        cameraGo.tag = "MainCamera";
        var sun = new GameObject("Soft light").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = 1.35f;
        sun.transform.rotation = Quaternion.Euler(50,-25,0);
        Spawn();
    }
    void Spawn()
    {
        if(customers.Count>=4) return;
        var t = new GameObject("Customer").transform;
        t.position = new Vector3(3.7f, .85f, -6.8f);
        var color = Mat(Color.HSVToRGB(Random.value,.52f,.95f));
        Box("Body", t.position, new Vector3(.8f,1.25f,.7f), color, t);
        Sphere("Head", t.position + Vector3.up*.87f, Vector3.one*.64f, amber, t);
        customers.Add(new Customer { body = t, patience = 32 });
    }
    void Update()
    {
        float dt = Time.deltaTime;
        var input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        if (Input.touchCount>0)
        {
            var touch=Input.GetTouch(0); var pos=touch.position;
            if(touch.phase==TouchPhase.Began && pos.x<Screen.width*.65f && pos.y<Screen.height*.55f)
            { dragging=true; joystickOrigin=pos; joystickCurrent=pos; }
            if(dragging) { joystickCurrent=pos; input=Vector2.ClampMagnitude((joystickCurrent-joystickOrigin)/70f,1); }
            if(touch.phase==TouchPhase.Ended || touch.phase==TouchPhase.Canceled) dragging=false;
        }
        var move=new Vector3(input.x,0,input.y);
        if(move.sqrMagnitude>1) move.Normalize();
        player.position+=move*5*dt;
        player.position=new Vector3(Mathf.Clamp(player.position.x,-6.7f,6.7f),.9f,Mathf.Clamp(player.position.z,-8.8f,8.2f));
        if(move.sqrMagnitude>.02f) player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(move),dt*12);
        if(Vector3.Distance(player.position,new Vector3(-3.7f,.9f,2.5f))<2.4f && ready<capacity)
        {
            cookTimer+=dt;
            if(cookTimer>=CookSeconds) { cookTimer=0;ready++;Message("Pizza ready! Take it to customers"); }
        }
        else cookTimer=0;
        if(Vector3.Distance(player.position,new Vector3(3.7f,.9f,2.5f))<2.5f && ready>0 && customers.Count>0)
        {
            ready--; cash+=10;served++;Save();
            Destroy(customers[0].body.gameObject);customers.RemoveAt(0);
            Message("Order served  +$10");
        }
        if(Vector3.Distance(player.position,new Vector3(0,.9f,-5.3f))<1.7f && cash>=upgradeCost)
        {
            cash-=upgradeCost;capacity++;upgradeCost+=20;Save();
            Message("Tray upgraded! Capacity: "+capacity);
            player.position+=Vector3.forward*1.8f;
        }
        spawnTimer+=dt;
        if(spawnTimer>5) {spawnTimer=0;Spawn();}
        for(int i=customers.Count-1;i>=0;i--)
        {
            var c=customers[i];c.patience-=dt;
            var target=new Vector3(3.7f,.85f, .35f-i*1.35f);
            c.body.position=Vector3.MoveTowards(c.body.position,target,dt*2);
            if(c.patience<=0) {Destroy(c.body.gameObject);customers.RemoveAt(i);Message("A customer left. Serve faster!");}
        }
    }
    void Message(string message) {toast=message;toastUntil=Time.time+2.5f;}
    void Save()
    {
        PlayerPrefs.SetInt("foodstreet.cash",cash);
        PlayerPrefs.SetInt("foodstreet.served",served);
        PlayerPrefs.SetInt("foodstreet.capacity",capacity);
        PlayerPrefs.SetInt("foodstreet.upgradeCost",upgradeCost);
        PlayerPrefs.Save();
    }
    void OnGUI()
    {
        float s=Mathf.Clamp(Screen.width/430f,.75f,1.6f);
        var title=new GUIStyle(GUI.skin.label) {fontSize=Mathf.RoundToInt(24*s),fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
        title.normal.textColor=Color.white;
        var small=new GUIStyle(title) {fontSize=Mathf.RoundToInt(16*s)};
        GUI.Box(new Rect(12,10,Screen.width-24,94*s),"");
        GUI.Label(new Rect(15,12,Screen.width-30,35*s),"FOOD STREET  •  PIZZA SHOP",title);
        GUI.Label(new Rect(15,48*s,Screen.width-30,40*s),"Cash $"+cash+"     Pizza "+ready+"/"+capacity+"     Served "+served,small);
        if(Time.time<toastUntil) GUI.Label(new Rect(0,Screen.height*.60f,Screen.width,40*s),toast,small);
        GUI.Label(new Rect(0,Screen.height*.72f,Screen.width,85*s),
            "MOVE: drag left side / WASD\nPINK: cook   •   GOLD: serve\nGREEN: upgrade $"+upgradeCost,small);
        if(dragging) GUI.Box(new Rect(joystickOrigin.x-38,Screen.height-joystickOrigin.y-38,76,76),"●");
    }
}
