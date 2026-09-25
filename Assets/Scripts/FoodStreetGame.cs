using System.Collections.Generic;
using UnityEngine;

// Self-contained, asset-free prototype. All world geometry is generated at runtime.
public class FoodStreetGame : MonoBehaviour
{
    class Customer { public Transform body; public float patience; }
    readonly List<Customer> customers = new List<Customer>();
    Transform player;
    Camera cam;
    Material floor, dark, pink, amber, green, white, pizza, brass, stone, wood, glass;
    float cookTimer, spawnTimer, toastUntil;
    int ready, cash, served, capacity = 2, upgradeCost = 25;
    string toast = "";
    Texture2D panelTexture;
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
        panelTexture=new Texture2D(1,1);
        panelTexture.SetPixel(0,0,new Color(.025f,.027f,.03f,.91f));panelTexture.Apply();
        cash = PlayerPrefs.GetInt("foodstreet.cash", 0);
        served = PlayerPrefs.GetInt("foodstreet.served", 0);
        capacity = PlayerPrefs.GetInt("foodstreet.capacity", 2);
        upgradeCost = PlayerPrefs.GetInt("foodstreet.upgradeCost", 25);
        floor = Mat(new Color(.075f,.075f,.078f)); dark = Mat(new Color(.025f,.03f,.036f));
        pink = Mat(new Color(.35f,.13f,.105f)); amber = Mat(new Color(.82f,.53f,.24f),.68f);
        green = Mat(new Color(.13f,.35f,.29f)); white = Mat(new Color(.83f,.79f,.68f));
        pizza = Mat(new Color(.78f,.29f,.105f)); brass=Mat(new Color(.62f,.43f,.2f),.82f);
        stone=Mat(new Color(.30f,.29f,.27f),.12f);wood=Mat(new Color(.17f,.085f,.055f),.08f);
        glass=Mat(new Color(.13f,.2f,.22f),.55f);
        RenderSettings.ambientLight = new Color(.38f,.35f,.31f);
        Box("Polished stone floor", Vector3.zero, new Vector3(15,.2f,20), floor);
        for(int x=-7;x<=7;x+=2) for(int z=-9;z<=9;z+=2)
            Box("Stone floor slab",new Vector3(x,.111f,z),new Vector3(1.94f,.012f,1.94f),stone);
        Box("Walnut feature wall", new Vector3(0,2.2f,9.9f), new Vector3(15,4.5f,.25f), wood);
        Box("Left wall", new Vector3(-7.4f,2.2f,0), new Vector3(.25f,4.5f,20), dark);
        Box("Right wall", new Vector3(7.4f,2.2f,0), new Vector3(.25f,4.5f,20), dark);
        for(int i=0;i<7;i++) Box("Brass wall inlay",new Vector3(-6.5f+i*2.2f,2.3f,9.71f),new Vector3(.045f,3.8f,.04f),brass);
        Box("Cook island walnut", new Vector3(-3.7f,.56f,2.5f), new Vector3(4,1.1f,2), wood);
        Box("Cook island marble top",new Vector3(-3.7f,1.15f,2.5f),new Vector3(4.2f,.12f,2.2f),stone);
        Box("Cook counter brass edging",new Vector3(-3.7f,1.06f,1.39f),new Vector3(4.2f,.035f,.04f),brass);
        Box("Service island walnut", new Vector3(3.7f,.56f,2.5f), new Vector3(4,1.1f,2), wood);
        Box("Service marble top",new Vector3(3.7f,1.15f,2.5f),new Vector3(4.2f,.12f,2.2f),stone);
        Box("Service brass edging",new Vector3(3.7f,1.06f,1.39f),new Vector3(4.2f,.035f,.04f),brass);
        Box("Upgrade station", new Vector3(0,.15f,-5.3f), new Vector3(2.5,.09f,2.1f), brass);
        Box("Upgrade center", new Vector3(0,.2f,-5.3f), new Vector3(2.2f,.03f,1.8f), dark);
        for (int i=0;i<3;i++)
        {
            var x=-5+i*5;
            Box("Pendant stem",new Vector3(x,4.1f,2),new Vector3(.055f,.7f,.055f),brass);
            Box("Pendant shade",new Vector3(x,3.72f,2),new Vector3(1.1f,.15f,1.1f),dark);
            Box("Pendant diffuser",new Vector3(x,3.62f,2),new Vector3(.85f,.035f,.85f),amber);
            var lamp=new GameObject("Warm pendant light").AddComponent<Light>();
            lamp.type=LightType.Point;lamp.color=new Color(1f,.73f,.46f);lamp.range=6;lamp.intensity=2.0f;
            lamp.transform.position=new Vector3(x,3.45f,2);
        }
        Box("Pizza oven",new Vector3(-3.7f,1.85f,8.6f),new Vector3(3.2f,2.5f,1.6f),dark);
        Box("Oven mouth",new Vector3(-3.7f,1.82f,7.77f),new Vector3(2.15f,1.05f,.08f),pizza);
        Box("Oven opening",new Vector3(-3.7f,1.73f,7.71f),new Vector3(1.7f,.55f,.08f),dark);
        Box("Oven brass crown",new Vector3(-3.7f,3.14f,7.7f),new Vector3(3.25f,.07f,.12f),brass);
        Box("Tall drinks cooler",new Vector3(4.6f,1.8f,8.8f),new Vector3(2.6f,3.4f,1.5f),dark);
        Box("Cooler glass",new Vector3(4.6f,1.85f,7.98f),new Vector3(2.2f,2.8f,.045f),glass);
        for(int i=0;i<3;i++) Box("Cooler shelf",new Vector3(4.6f,.95f+i*.8f,7.94f),new Vector3(2.1f,.06f,.08f),brass);
        Sphere("Fresh pizza",new Vector3(-3.7f,1.28f,2.5f),new Vector3(1.25f,.12f,1.25f),pizza);
        for(int i=0;i<7;i++) Sphere("Topping",new Vector3(-3.7f+Random.Range(-.4f,.4f),1.36f,2.5f+Random.Range(-.4f,.4f)),new Vector3(.14f,.035f,.14f),green);
        Sign("FOOD STREET",new Vector3(0,3.26f,9.54f),.4f,white);
        Sign("PIZZA KITCHEN",new Vector3(-3.7f,2.9f,7.65f),.13f,amber);
        player = new GameObject("Player").transform;
        player.position = new Vector3(0,.9f,-2);
        Box("Chef uniform", player.position, new Vector3(.72f,1.2f,.48f), dark, player);
        Box("Apron", player.position+new Vector3(0,-.1f,-.26f),new Vector3(.61f,.9f,.05f),white,player);
        Sphere("Head", player.position + Vector3.up*.86f, new Vector3(.58f,.64f,.58f), amber, player);
        Box("Chef cap", player.position + Vector3.up*1.24f, new Vector3(.69f,.2f,.7f), dark, player);
        Box("Left leg",player.position+new Vector3(-.2f,-.82f,0),new Vector3(.21f,.48f,.25f),dark,player);
        Box("Right leg",player.position+new Vector3(.2f,-.82f,0),new Vector3(.21f,.48f,.25f),dark,player);
        // Child meshes retain their world positions; move root only after creation.
        var cameraGo = new GameObject("Camera"); cam = cameraGo.AddComponent<Camera>();
        cam.transform.position = new Vector3(0,16,-14);
        cam.transform.rotation = Quaternion.Euler(49,0,0);
        cam.fieldOfView = 55; cam.backgroundColor = new Color(.045f,.045f,.05f);
        cameraGo.tag = "MainCamera";
        var sun = new GameObject("Soft light").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity = .85f;
        sun.color=new Color(1f,.85f,.7f);
        sun.transform.rotation = Quaternion.Euler(50,-25,0);
        Spawn();
    }
    void Sign(string content,Vector3 pos,float size,Material material)
    {
        var label=new GameObject("Interior sign: "+content).AddComponent<TextMesh>();
        label.text=content;label.fontSize=64;label.characterSize=size/6f;
        label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;
        label.color=material.color;label.transform.position=pos;
        label.transform.rotation=Quaternion.Euler(0,180,0);
    }
    void Spawn()
    {
        if(customers.Count>=4) return;
        var t = new GameObject("Customer").transform;
        t.position = new Vector3(3.7f, .85f, -6.8f);
        var color = Mat(Color.HSVToRGB(Random.value,.22f,.48f));
        Box("Coat", t.position, new Vector3(.68f,1.25f,.5f), color, t);
        Sphere("Head", t.position + Vector3.up*.87f, Vector3.one*.57f, amber, t);
        Box("Left leg",t.position+new Vector3(-.19f,-.74f,0),new Vector3(.18f,.42f,.23f),dark,t);
        Box("Right leg",t.position+new Vector3(.19f,-.74f,0),new Vector3(.18f,.42f,.23f),dark,t);
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
        var title=new GUIStyle(GUI.skin.label) {fontSize=Mathf.RoundToInt(22*s),fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};
        title.normal.textColor=new Color(.95f,.83f,.6f);
        var small=new GUIStyle(GUI.skin.label) {fontSize=Mathf.RoundToInt(14*s),alignment=TextAnchor.MiddleLeft,wordWrap=true};
        small.normal.textColor=new Color(.91f,.9f,.83f);
        var centered=new GUIStyle(small) {alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
        GUI.DrawTexture(new Rect(0,0,Screen.width,110*s),panelTexture);
        GUI.Label(new Rect(20*s,8*s,Screen.width-40*s,35*s),"FOOD STREET",title);
        GUI.Label(new Rect(20*s,42*s,Screen.width-40*s,28*s),"THE NIGHT MARKET  /  PIZZA KITCHEN",small);
        GUI.Label(new Rect(20*s,71*s,Screen.width-40*s,29*s),"$"+cash+" CASH     "+ready+"/"+capacity+" READY     "+served+" SERVED",small);
        if(Time.time<toastUntil)
        {
            GUI.DrawTexture(new Rect(0,Screen.height*.61f,Screen.width,42*s),panelTexture);
            GUI.Label(new Rect(0,Screen.height*.61f,Screen.width,42*s),toast,centered);
        }
        GUI.DrawTexture(new Rect(0,Screen.height-100*s,Screen.width,100*s),panelTexture);
        GUI.Label(new Rect(18*s,Screen.height-98*s,Screen.width-36*s,85*s),
            "DRAG TO MOVE   •   PINK COUNTER: COOK\nGOLD COUNTER: SERVE   •   REAR PAD: UPGRADE $"+upgradeCost,small);
        if(dragging) GUI.Box(new Rect(joystickOrigin.x-38,Screen.height-joystickOrigin.y-38,76,76),"●");
    }
}
