using System.Collections.Generic;
using UnityEngine;

// An original idle restaurant prototype. Geometry is temporary and generated at runtime.
public sealed class PizzaMasterGame : MonoBehaviour
{
    sealed class Guest { public Transform avatar; public float patience; }
    readonly List<Guest> line = new List<Guest>();
    Transform player, helper, tray;
    readonly List<GameObject> carriedPizzas = new List<GameObject>();
    Material cream, tile, tomato, mint, blue, crust, topping, skin, uniform, black, gold, wood, leaf, glass;
    Texture2D panel;
    float spawnClock, cookClock, serveClock, helperClock, messageUntil;
    int money, served, carry, capacity, level, staff, speedLevel;
    string message = "MOVE TO THE OVEN TO MAKE PIZZA";
    Vector2 dragStart, dragNow;
    int dragFinger = -1;
    Vector3 velocity;

    Material Make(Color color)
    {
        var baseMaterial = Resources.Load<Material>("PizzaMasterBase");
        var material = baseMaterial != null ? new Material(baseMaterial) :
            new Material(Shader.Find("Standard") ?? Shader.Find("Sprites/Default"));
        material.color = color;
        return material;
    }
    Transform Cube(string name, Vector3 pos, Vector3 size, Material mat, Transform parent = null)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name=name; obj.transform.position=pos; obj.transform.localScale=size;
        obj.GetComponent<Renderer>().sharedMaterial=mat;
        if(parent) obj.transform.SetParent(parent,true);
        return obj.transform;
    }
    Transform Ball(string name,Vector3 pos,Vector3 size,Material mat,Transform parent=null)
    {
        var obj=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name=name;obj.transform.position=pos;obj.transform.localScale=size;
        obj.GetComponent<Renderer>().sharedMaterial=mat;
        if(parent)obj.transform.SetParent(parent,true);
        return obj.transform;
    }
    Transform Person(string name, Vector3 pos, Material shirt)
    {
        var root=new GameObject(name).transform;root.position=pos;
        Cube("Shirt",pos,new Vector3(.64f,1.05f,.46f),shirt,root);
        Ball("Head",pos+Vector3.up*.79f,new Vector3(.56f,.57f,.55f),skin,root);
        Cube("Left leg",pos+new Vector3(-.18f,-.7f,0),new Vector3(.2f,.47f,.24f),black,root);
        Cube("Right leg",pos+new Vector3(.18f,-.7f,0),new Vector3(.2f,.47f,.24f),black,root);
        return root;
    }
    void Label(string text,Vector3 pos,Color color,float scale)
    {
        var mesh=new GameObject(text).AddComponent<TextMesh>();
        mesh.text=text;mesh.anchor=TextAnchor.MiddleCenter;mesh.alignment=TextAlignment.Center;
        mesh.fontSize=64;mesh.characterSize=scale;mesh.color=color;
        mesh.transform.position=pos;
        mesh.transform.rotation=Quaternion.identity;
    }
    void DecorateRestaurant()
    {
        // Warm, bright storefront. All decorative geometry is original and generated here.
        Cube("Left wall",new Vector3(-7.55f,1.3f,0),new Vector3(.15f,2.6f,19.5f),cream);
        Cube("Right wall",new Vector3(7.55f,1.3f,0),new Vector3(.15f,2.6f,19.5f),cream);
        Cube("Front entrance strip",new Vector3(0,.12f,-9.6f),new Vector3(14.8f,.08f,.38f),tomato);
        for(int i=0;i<4;i++)
        {
            float x=-5.5f+i*3.7f;
            Cube("Wall window",new Vector3(x,2.15f,9.62f),new Vector3(2.75f,1.15f,.08f),glass);
            Cube("Window sill",new Vector3(x,1.52f,9.48f),new Vector3(2.85f,.12f,.3f),wood);
        }
        // A striped awning marks the shop, and two planter rows soften the entrance.
        for(int i=0;i<8;i++)
            Cube("Awning stripe",new Vector3(-6.25f+i*1.8f,4.4f,8.6f),
                new Vector3(1.8f,.16f,1.6f),i%2==0?tomato:uniform);
        for(int i=0;i<4;i++)
        {
            float z=-7+i*4.1f;
            foreach(float x in new[]{-6.65f,6.65f})
            {
                Cube("Planter",new Vector3(x,.44f,z),new Vector3(.65f,.72f,.65f),wood);
                Ball("Herb",new Vector3(x,.93f,z),new Vector3(.8f,.85f,.8f),leaf);
            }
        }
        // Distinct preparation table and service counter make the route legible.
        Cube("Prep table",new Vector3(-3.8f,.65f,5.25f),new Vector3(3.1f,.16f,1.25f),wood);
        for(int i=0;i<3;i++)
        {
            float x=-4.5f+i*.7f;
            var dough=Ball("Fresh dough",new Vector3(x,.81f,5.25f),new Vector3(.48f,.09f,.48f),crust);
            Ball("Sauce",dough.position+Vector3.up*.05f,new Vector3(.35f,.035f,.35f),topping);
        }
        Cube("Cash register",new Vector3(3.8f,1.22f,2.9f),new Vector3(.66f,.32f,.43f),black);
        Cube("Register screen",new Vector3(3.8f,1.34f,2.67f),new Vector3(.46f,.17f,.03f),glass);
        for(int i=0;i<3;i++)
        {
            float x=1.45f+i*2.35f;
            Cube("Dining table",new Vector3(x,.65f,6.3f),new Vector3(1.25f,.12f,1.15f),wood);
            Cube("Dining table leg",new Vector3(x,.35f,6.3f),new Vector3(.14f,.55f,.14f),black);
            Cube("Dining stool",new Vector3(x,.38f,5.12f),new Vector3(.6f,.15f,.6f),mint);
        }
        Label("BAKE",new Vector3(-3.8f,.26f,.75f),Color.white,.045f);
        Label("SERVE",new Vector3(3.8f,.26f,.75f),Color.white,.045f);
    }
    void Start()
    {
        Application.targetFrameRate=60;
        money=PlayerPrefs.GetInt("pm.money",0);served=PlayerPrefs.GetInt("pm.served",0);
        capacity=PlayerPrefs.GetInt("pm.capacity",2);level=PlayerPrefs.GetInt("pm.level",1);
        staff=PlayerPrefs.GetInt("pm.staff",0);speedLevel=PlayerPrefs.GetInt("pm.speed",0);
        int last=PlayerPrefs.GetInt("pm.last",0);
        if(last>0 && staff>0)
        {
            int elapsed=Mathf.Clamp((int)(System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()-last),0,14400);
            money+=Mathf.Min(200,elapsed/60*staff);
        }
        panel=new Texture2D(1,1);panel.SetPixel(0,0,new Color(1,1,1,.94f));panel.Apply();
        cream=Make(new Color(.98f,.94f,.84f));tile=Make(new Color(.93f,.9f,.82f));
        tomato=Make(new Color(.9f,.27f,.18f));mint=Make(new Color(.35f,.68f,.5f));
        blue=Make(new Color(.4f,.67f,.78f));crust=Make(new Color(.93f,.66f,.32f));
        topping=Make(new Color(.67f,.13f,.11f));skin=Make(new Color(.81f,.55f,.37f));
        uniform=Make(new Color(.95f,.95f,.89f));black=Make(new Color(.23f,.26f,.29f));
        gold=Make(new Color(.98f,.78f,.27f));wood=Make(new Color(.69f,.42f,.27f));
        leaf=Make(new Color(.22f,.57f,.32f));glass=Make(new Color(.62f,.85f,.94f));
        RenderSettings.ambientLight=new Color(.85f,.84f,.8f);
        Cube("Light restaurant floor",Vector3.zero,new Vector3(15,.2f,20),cream);
        for(int x=-6;x<=6;x+=2)for(int z=-8;z<=8;z+=2)
            Cube("Floor tile",new Vector3(x,.115f,z),new Vector3(1.94f,.02f,1.94f),tile);
        Cube("Back wall",new Vector3(0,2.1f,9.8f),new Vector3(15,4.4f,.2f),cream);
        Cube("Pizza Master sign",new Vector3(0,3.35f,9.65f),new Vector3(6.8f,.72f,.09f),tomato);
        Label("PIZZA MASTER",new Vector3(0,3.38f,9.56f),Color.white,.065f);
        Counter("OVEN",new Vector3(-3.8f,.53f,2.8f),tomato);
        Counter("SERVICE",new Vector3(3.8f,.53f,2.8f),blue);
        Cube("Stone oven",new Vector3(-3.8f,1.62f,8.3f),new Vector3(3.2f,2.4f,1.6f),tile);
        Cube("Oven mouth",new Vector3(-3.8f,1.65f,7.46f),new Vector3(2.2f,.9f,.07f),black);
        Cube("Oven flame",new Vector3(-3.8f,1.33f,7.39f),new Vector3(1.7f,.15f,.05f),gold);
        Cube("Oven pad",new Vector3(-3.8f,.17f,.75f),new Vector3(2.1f,.09f,1.7f),tomato);
        Cube("Service pad",new Vector3(3.8f,.17f,.75f),new Vector3(2.1f,.09f,1.7f),blue);
        Cube("Tray upgrade",new Vector3(-4,.16f,-5.2f),new Vector3(2.35f,.08f,1.8f),gold);
        Cube("Staff hiring",new Vector3(0,.16f,-5.2f),new Vector3(2.35f,.08f,1.8f),mint);
        Cube("Kitchen upgrade",new Vector3(4,.16f,-5.2f),new Vector3(2.35f,.08f,1.8f),tomato);
        DecorateRestaurant();
        for(int n=0;n<3;n++)
        {
            var p=Ball("Pizza",new Vector3(-4.45f+n*.65f,1.13f,2.8f),new Vector3(.55f,.09f,.55f),crust);
            Ball("Sauce",p.position+Vector3.up*.06f,new Vector3(.42f,.04f,.42f),topping);
            Ball("Basil",p.position+new Vector3(.12f,.095f,.1f),new Vector3(.09f,.03f,.12f),leaf);
        }
        player=Person("Player",new Vector3(0,.83f,-1.7f),tomato);
        Cube("Apron",player.position+new Vector3(0,-.06f,-.25f),new Vector3(.5f,.7f,.05f),uniform,player);
        tray=Cube("Carry tray",player.position+new Vector3(0,.68f,-.55f),new Vector3(.9f,.06f,.7f),black,player);
        if(staff>0)CreateHelper();
        var camera=Camera.main;
        if(camera==null)
        {
            var cameraObj=new GameObject("Main Camera");cameraObj.tag="MainCamera";
            camera=cameraObj.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=12;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.7f,.86f,.9f);
            camera.transform.position=new Vector3(0,17,-12);
            camera.transform.rotation=Quaternion.Euler(53,0,0);
        }
        var sun=new GameObject("Sunlight").AddComponent<Light>();
        sun.type=LightType.Directional;sun.color=new Color(1f,.95f,.85f);sun.intensity=1.1f;
        sun.transform.rotation=Quaternion.Euler(48,-28,0);
        SpawnGuest();Save();
    }
    void Counter(string name,Vector3 pos,Material accent)
    {
        Cube(name+" counter",pos,new Vector3(3.4f,1.1f,1.8f),uniform);
        Cube(name+" colored trim",pos+new Vector3(0,.55f,0),new Vector3(3.55f,.13f,1.96f),accent);
    }
    void CreateHelper()
    {
        if(helper!=null)return;
        helper=Person("Auto chef",new Vector3(-5.5f,.83f,4.3f),mint);
        Cube("Apron",helper.position+new Vector3(0,-.06f,-.25f),new Vector3(.5f,.7f,.05f),uniform,helper);
    }
    void SpawnGuest()
    {
        if(line.Count>=5)return;
        var color=Make(Color.HSVToRGB(Random.value,.3f,.85f));
        var guest=Person("Customer",new Vector3(4,.83f,-8.5f),color);
        line.Add(new Guest{avatar=guest,patience=50});
    }
    void Save()
    {
        PlayerPrefs.SetInt("pm.money",money);PlayerPrefs.SetInt("pm.served",served);
        PlayerPrefs.SetInt("pm.capacity",capacity);PlayerPrefs.SetInt("pm.level",level);
        PlayerPrefs.SetInt("pm.staff",staff);PlayerPrefs.SetInt("pm.speed",speedLevel);
        PlayerPrefs.SetInt("pm.last",(int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        PlayerPrefs.Save();
    }
    void Notice(string text){message=text;messageUntil=Time.time+2.8f;}
    void RefreshTray()
    {
        foreach(var pizza in carriedPizzas) Destroy(pizza);
        carriedPizzas.Clear();
        // Stack a visible pizza for each order in the tray.
        for(int i=0;i<carry;i++)
        {
            var pizza=Ball("Carried pizza",tray.position+new Vector3(0,.09f+i*.11f,0),
                new Vector3(.55f,.08f,.55f),crust,tray);
            Ball("Tomato sauce",pizza.position+Vector3.up*.045f,
                new Vector3(.43f,.025f,.43f),topping,pizza);
            Ball("Basil",pizza.position+new Vector3(.12f,.065f,.08f),
                new Vector3(.08f,.025f,.11f),leaf,pizza);
            carriedPizzas.Add(pizza.gameObject);
        }
    }
    void Update()
    {
        float dt=Time.deltaTime;
        Vector2 input=new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        if(Input.touchCount==0 && Input.GetMouseButton(0))
        {
            if(dragFinger<0){dragFinger=0;dragStart=Input.mousePosition;}
            input=Vector2.ClampMagnitude(((Vector2)Input.mousePosition-dragStart)/90f,1);
        }
        for(int i=0;i<Input.touchCount;i++)
        {
            var t=Input.GetTouch(i);
            if(t.phase==TouchPhase.Began && dragFinger<0 && t.position.x<Screen.width*.75f)
            {dragFinger=t.fingerId;dragStart=t.position;dragNow=t.position;}
            if(t.fingerId!=dragFinger)continue;
            if(t.phase==TouchPhase.Ended||t.phase==TouchPhase.Canceled)dragFinger=-1;
            else{dragNow=t.position;input=Vector2.ClampMagnitude((dragNow-dragStart)/90f,1);}
        }
        if(Input.touchCount==0 && !Input.GetMouseButton(0))dragFinger=-1;
        // The camera looks into the restaurant from the entrance. Match drag-up
        // to the player's forward route as seen on the phone.
        Vector3 goal=new Vector3(input.x,0,-input.y);
        velocity=Vector3.Lerp(velocity,goal*5.2f,Mathf.Clamp01(dt*10));
        player.position+=velocity*dt;
        player.position=new Vector3(Mathf.Clamp(player.position.x,-6.9f,6.9f),.83f,Mathf.Clamp(player.position.z,-8.7f,7f));
        if(velocity.sqrMagnitude>.05f)player.rotation=Quaternion.Slerp(player.rotation,Quaternion.LookRotation(velocity),dt*10);
        if(Vector3.Distance(player.position,new Vector3(-3.8f,.83f,.75f))<1.7f&&carry<capacity)
        {
            cookClock+=dt;
            if(cookClock>=Mathf.Max(.9f,2.6f-speedLevel*.35f))
            {cookClock=0;carry++;RefreshTray();Notice("PIZZA READY  •  TAKE IT TO SERVICE");}
        }
        else cookClock=0;
        serveClock-=dt;
        if(serveClock<=0&&carry>0&&line.Count>0&&Vector3.Distance(player.position,new Vector3(3.8f,.83f,.75f))<1.8f)
        {serveClock=.8f;carry--;RefreshTray();Serve();}
        if(helper!=null)
        {
            helperClock+=dt;
            helper.position=new Vector3(-5.5f+Mathf.Sin(Time.time*1.5f)*.35f,.83f,4.3f);
            if(helperClock>=Mathf.Max(2f,6f-staff*.65f))
            {helperClock=0;if(line.Count>0){Serve();Notice("STAFF SERVED AN ORDER");}}
        }
        if(Vector3.Distance(player.position,new Vector3(-4,.83f,-5.2f))<1.4f&&money>=capacity*15)
        {money-=capacity*15;capacity++;player.position+=Vector3.forward*1.5f;Notice("TRAY CAPACITY +1");Save();}
        if(Vector3.Distance(player.position,new Vector3(0,.83f,-5.2f))<1.4f&&money>=40+staff*35)
        {money-=40+staff*35;staff++;CreateHelper();player.position+=Vector3.forward*1.5f;Notice("STAFF HIRED");Save();}
        if(Vector3.Distance(player.position,new Vector3(4,.83f,-5.2f))<1.4f&&money>=50+speedLevel*35)
        {money-=50+speedLevel*35;speedLevel++;player.position+=Vector3.forward*1.5f;Notice("OVEN UPGRADED");Save();}
        spawnClock+=dt;
        if(spawnClock>=Mathf.Max(2.3f,5f-level*.25f)){spawnClock=0;SpawnGuest();}
        for(int i=line.Count-1;i>=0;i--)
        {
            var guest=line[i];guest.patience-=dt;
            guest.avatar.position=Vector3.MoveTowards(guest.avatar.position,new Vector3(3.8f,.83f,-.5f-i*1.3f),dt*2.4f);
            if(guest.patience<=0){Destroy(guest.avatar.gameObject);line.RemoveAt(i);}
        }
    }
    void Serve()
    {
        var guest=line[0];line.RemoveAt(0);Destroy(guest.avatar.gameObject);
        money+=10+level*2;served++;
        int next=served/15+1;
        if(next>level){level=next;Notice("NEW SHOP LEVEL "+level);}
        Save();
    }
    void OnApplicationPause(bool paused){if(paused)Save();}
    void OnApplicationQuit(){Save();}
    void OnGUI()
    {
        float s=Mathf.Clamp(Screen.width/420f,.8f,1.65f);
        var body=new GUIStyle(GUI.skin.label){fontSize=Mathf.RoundToInt(14*s),fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};
        body.normal.textColor=Color.white;
        var center=new GUIStyle(body){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
        var white=new GUIStyle(body){alignment=TextAnchor.MiddleCenter};
        white.normal.textColor=new Color(.25f,.24f,.22f);
        var header=new Rect(12*s,10*s,Screen.width-24*s,48*s);
        var oldColor=GUI.color;
        GUI.color=new Color(.18f,.18f,.2f,.86f);GUI.DrawTexture(header,Texture2D.whiteTexture);
        GUI.color=oldColor;
        GUI.Label(new Rect(24*s,15*s,Screen.width-48*s,36*s),
            "$"+money+"    LEVEL "+level+"    PIZZA "+carry+"/"+capacity+"    QUEUE "+line.Count,body);
        if(cookClock>0)
        {
            float duration=Mathf.Max(.9f,2.6f-speedLevel*.35f);
            GUI.color=new Color(.96f,.65f,.2f);
            GUI.DrawTexture(new Rect(12*s,61*s,(Screen.width-24*s)*Mathf.Clamp01(cookClock/duration),7*s),Texture2D.whiteTexture);
            GUI.color=oldColor;
        }
        if(Time.time<messageUntil)
        {
            GUI.color=new Color(.18f,.18f,.2f,.8f);
            GUI.DrawTexture(new Rect(24*s,Screen.height*.74f,Screen.width-48*s,38*s),Texture2D.whiteTexture);
            GUI.color=oldColor;
            GUI.Label(new Rect(24*s,Screen.height*.74f,Screen.width-48*s,38*s),message,center);
        }
        if(Time.time<8f)
        {
            GUI.color=new Color(1f,1f,1f,.85f);
            GUI.DrawTexture(new Rect(35*s,Screen.height-59*s,Screen.width-70*s,38*s),Texture2D.whiteTexture);
            GUI.color=oldColor;
            GUI.Label(new Rect(35*s,Screen.height-59*s,Screen.width-70*s,38*s),"DRAG TO MOVE · RED: BAKE · BLUE: SERVE",white);
        }
    }
}
