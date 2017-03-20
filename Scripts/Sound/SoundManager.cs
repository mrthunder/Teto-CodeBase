using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// All the events exist in the wwise project
/// </summary>
public enum SoundEvents
{
    ///<summary> Play the Dark/Light Background </summary>
    Play_BG,
    ///<summary> Stop the Dark/Light Background </summary>
    Stop_BG,
    ///<summary> Play the Restoration Background (ONLY DURING CINEMATICS) </summary>
    Play_Restoration_BG ,
    ///<summary> Stop the Restoration Background (STOP AFTER CINEMATICS) </summary>
    Stop_Restoration_BG ,
    ///<summary> Play the backgound when the enemies are close to Teto (WOLFS CHASING YOU) </summary>
    Play_Stalk_BG,
    ///<summary> Stop the backgound when the enemies are close to Teto (WOLFS NOT CHASING YOU) </summary>
    Stop_Stalk_BG,
    ///<summary> Play the detaching sound of the platform, followed by a loop (RAISING) </summary>
    Play_Raising_FP,
    ///<summary> Play the sound for the platform while floating (FLOATING LOOP) </summary>
    Play_Floating_Platform,
    ///<summary> Stop the sound for the platform while floating (STOP FLOATING LOOP) </summary>
    Stop_Floating_Platform,
    ///<summary> Play the sound for the activation of the pressure plate (PRESSURE PLATE ON) </summary>
    Play_PP_Activation,
    ///<summary> Play the sound for the deactivation of the pressure plate (PRESSURE PLATE OFF) </summary>
    Play_PP_Deactivation,
    ///<summary> Play the sound for a small object moving (DOORS OPEN/CLOSE) </summary>
    Play_Small_Drag,
    ///<summary> Play the sound for a big object moving (STAIRS AND BIG BLOCKS) </summary>
    Play_Big_Drag,
    ///<summary> Stop the sound for a small object moving (DOORS OPEN/CLOSE) </summary>
    Stop_Small_Drag,
    ///<summary> Stop the sound for a big object moving (STAIRS AND BIG BLOCKS) </summary>
    Stop_Big_Drag,
    ///<summary> Play the sound for the restorating grass </summary>
    Play_Restoration_Grass,
    ///<summary> Stop the sound for the restorating grass </summary>
    Stop_Restoration_Grass,
    ///<summary> Play the sound for the restorating tree </summary>
    Play_Restoration_Tree,
    ///<summary> Play the loop of the light vein (LIGHT VEIN) </summary>
    Play_LV_Emitter ,
    ///<summary> Stop the light vein from playing (LIGHT VEIN) </summary>
    Stop_LV_Emitter ,
    ///<summary> Play the sound for the activation of the light vein (LIGHT VEIN) </summary>
    Play_LV_Activation,
    ///<summary> Play the sound of the light beam (LIGHT ENERGY BEAM END OF LEVEL)</summary>
    Play_EB_Emitter ,
    ///<summary> Stop the light beam from playing (LIGHT ENERGY BEAM END OF LEVEL) </summary>
    Stop_EB_Emitter ,
    ///<summary> Play the sound for the activation of the light beam (LIGHT ENERGY BEAM END OF LEVEL) </summary>
    Play_EB_Activation ,
    ///<summary> Play the sound for Teto's jump</summary>
    Play_Teto_Jump ,
    ///<summary> Play the sound for Teto's landing </summary>
    Play_Teto_Land ,
    ///<summary> Play one footstep for Teto </summary>
    Play_Teto_Footsteps ,
    ///<summary> Play the sound for the Dash </summary>
    Play_Teto_Dash ,
    ///<summary> Play the sound of Teto's aura </summary>
    Play_Teto_Aura,
    ///<summary> Stop the sound of Teto's aura </summary>
    Stop_Teto_Aura ,
    ///<summary> Play the sound for the Pulse </summary>
    Play_Teto_Pulse ,
    ///<summary> Play a sound when Teto it's hit </summary>
    Play_Teto_Hit ,
    ///<summary> Play a sound for Teto's Spawn </summary>
    Play_Teto_Spawn ,
    ///<summary> Play the sound for Teto's dopple </summary>
    Play_Teto_Dopple ,
    ///<summary> Play the sound when wolf gets hit </summary>
    Play_Wolf_Hit ,
    ///<summary> Play the sound for the wolf growling </summary>
    Play_Wolf_Growl ,
    ///<summary> Play one footstep for the Wolf </summary>
    Play_Wolf_Footsteps ,
    ///<summary> Play the sound for the Wolf's jump </summary>
    Play_Wolf_Jump ,
    ///<summary> Play the sound for the Wolf's landing </summary>
    Play_Wolf_Land ,
    ///<summary> Play the sound of the wolf attacking </summary>
    Play_Wolf_Attack ,
    ///<summary> Play a sound for the Shrine activation (for tutorial) </summary>
    Play_Shrine_Activation,
    ///<summary> Play the sound for the ghost moving around </summary>
    Play_Ghost_Movement ,
    ///<summary> Stop the sound for the ghost moving around </summary>
    Stop_Ghost_Movement ,
    /// <summary>
    /// Play the sound when the mouse go on top of any option in the menu (or changing beetween option with joystick)
    /// </summary>
    Play_UI_Navigation ,
    /// <summary>
    /// Play the sound when the user go back with the tab in the menu
    /// </summary>
    Play_UI_Return ,
    /// <summary>
    /// Play the sound when the user select an option in the menu
    /// </summary>
    Play_UI_Select ,
    /// <summary>
    /// Play the Wind in the background
    /// </summary>
    Play_Wind ,
    /// <summary>
    /// Stop the Wind from playing
    /// </summary>
    Stop_Wind ,
    /// <summary>
    /// Play the sound for the raven looking around
    /// </summary>
    Play_Rvn_Observation ,
    /// <summary>
    /// Stop the sound for the raven looking around
    /// </summary>
    Stop_Rvn_Observation,
    /// <summary>
    /// Play the sound when the Raven spot you
    /// </summary>
    Play_Rvn_Alert,
    /// <summary>
    /// Play the sound when the raven shoots you (ATTACHED TO PROJECTILE)
    /// </summary>
    Play_Rvn_Attack,
    /// <summary>
    /// Play the sound for the explosion shooted by the ravens
    /// </summary>
    Play_Rvn_Explosion,
    /// <summary>
    /// Play the sound when Teto touch a ghost
    /// </summary>
    Play_Ghost_Absorb,
    /// <summary>
    /// Play the sound for the LIGHT SPIRIT ABSORBED
    /// </summary>
    Play_LS_Absorb,
    /// <summary>
    /// Play a sound when Teto doesn't have enough energy to use an ability
    /// </summary>
    Play_Low_Energy,
    /// <summary>
    /// Play the sniffing sound during Teto's idle
    /// </summary>
    Play_Teto_Sniff,
    /// <summary>
    /// Play the sound for the Vine Bridge growing
    /// </summary>
    Play_Vine_Bridge,
    /// <summary>
    /// Stop the sound for the Vine Bridge growing
    /// </summary>
    Stop_Vine_Bridge,
    /// <summary>
    /// Play the sound for the falling platform
    /// </summary>
    Play_Weak_Platform,
    /// <summary>
    /// Play the sound for the wolf Howling animation
    /// </summary>
    Play_Wolf_Howling,
    Play_Music,
    Stop_Music,
    Play_Meteor,
    Stop_Meteor




}

/// <summary>
/// All the game parameters or RTPC in the wwise project
/// </summary>
public enum GameParameters
{
    ///<summary>0 = Dark wind 1 = Light wind (for the correspondent locations)</summary>
    Wind_Type ,
    ///<summary>0 = Lowest Speed 1 = Fastest Speed</summary>
    Teto_Speed ,
    ///<summary>0 = Lowest Health 10 = Full Health</summary>
    Teto_Health ,
    ///<summary>0 = Lowest Speed 1 = Fastest Speed</summary>
    Wolf_Speed ,
    ///<summary>0 = Dark BG 1 = Light BG</summary>
    BG_Type ,
    /// <summary>
    /// 0 = Mute 1= Full Volume
    /// </summary>
    BG,
    /// <summary>
    /// 0 = Mute 1= Full Volume
    /// </summary>
    SFX,
    /// <summary>
    /// 0 = Mute 1= Full Volume
    /// </summary>
    Music,
    /// <summary>
    /// 0 = Distant 1 = Close
    /// </summary>
    Distance


}



public enum MusicState
{
    Danger,
    Health,
    Idle,
    Light,
    Menu,
    None
}

public enum Surfaces { Rock, Dirt, Grass_Low, Grass_Mid, Grass_High }

public enum BusName { Dark_Rvrb,Light_Rvrb,Temple_Rvrb}

public class SoundManager : Singleton<SoundManager>
{

    internal string footstepsEvent = string.Empty;


    private MusicState m_currentState = MusicState.None;

    public MusicState CurrentState
    {
        get
        {
            return m_currentState;
        }
    }

    /// <summary>
    /// Who call the stalk bg
    /// </summary>
    private GameObject _stalkBGOwner = null;
    
    /// <summary>
    /// Which wolf calls the stalk BG
    /// </summary>
    public GameObject m_StalkBGOwner
    {
        get
        {
            return _stalkBGOwner;
        }
    }

    GameObject musicObject;

    void Start()
    {
        
        SetGameParameter(GameParameters.BG_Type, 0);
    }

    public void SetGameParameter(GameParameters parameter, float value, GameObject from=null)
    {
        string name = System.Enum.GetName(typeof(GameParameters), parameter);
        if(from)
        {
            AkSoundEngine.SetRTPCValue(name, value, from);
        }
        else
        {
            AkSoundEngine.SetRTPCValue(name, value);
        }
        
        
       
      
    }

    public void PlayEvent(string name, GameObject from)
    {
        object check = System.Enum.Parse(typeof(SoundEvents), name);

        ControlStalkBG(from, (SoundEvents)check);


        if (check != null)
        {
            
            uint akCode = AkSoundEngine.PostEvent(name, from, 1);
            if (akCode == 0)
            {
                Debug.LogError(string.Format("The event {0} don't exist.\nGameObject: {1}", name, from));
            }
            
        }
        else
        {
            Debug.LogError("The event " + name + " don't exist", from);
        }

    }

   

    public void PlayEvent(SoundEvents name, GameObject from)
    {
        if(name == SoundEvents.Play_Music && !musicObject)
        {
            musicObject = from;
        }

        ControlStalkBG(from, name);
        string eventName = System.Enum.GetName(typeof(SoundEvents), name);
        if(string.IsNullOrEmpty(eventName))
        {
            throw new System.ArgumentException("Name cannot be empty or null", "name");
        }
        else
        {
            uint akCode = AkSoundEngine.PostEvent(eventName, from, 1);
            
            
        }
       
    }

    void ControlStalkBG(GameObject owner,SoundEvents sound)
    {
        if (sound == SoundEvents.Play_Stalk_BG || sound == SoundEvents.Play_Wolf_Howling)
        {
            if (m_currentState != MusicState.Health)
            {
                SetState(MusicState.Danger);
               
            }
        }
        if (sound == SoundEvents.Play_Stalk_BG)
        {
            if(_stalkBGOwner!=null)
            {
                PlayEvent(SoundEvents.Stop_Stalk_BG, _stalkBGOwner);
            }
            _stalkBGOwner = owner;
        }
        

    }

    void SwitchStates(SoundEvents events)
    {
        if (events == SoundEvents.Play_Stalk_BG)
        {
            if (m_currentState != MusicState.Health)
            {
                SetState(MusicState.Danger);
            }
        }
        else if (events == SoundEvents.Stop_Stalk_BG)
        {
            if (m_currentState != MusicState.Health)
            {
                SetState(MusicState.Idle);
            }
        }
    }

    public void PlayTrigger(GameObject from, params string[] names)
    {
        if(names.Length>0)
        {
            foreach(string name in names)
            {
                AKRESULT result = AKRESULT.AK_Cancelled;
                int i = 0;
                do
                {
                    if (i >= 4)
                    {
                        Debug.LogError("Post Trigger Fail : " + result.ToString());
                    }
                    result = AkSoundEngine.PostTrigger(name, musicObject);
                    i++;
                } while (result != AKRESULT.AK_Success);
            }
        }
    }

    /// <summary>
    /// Set the music state
    /// </summary>
    /// <param name="state">Music state from wwise</param>
    /// <param name="stateOverride">if can override the song</param>
    public void SetState(MusicState state, bool stateOverride = false)
    {
        if (state == CurrentState) return;
        if(CurrentState == MusicState.Light && stateOverride)
        {
            SetMusic(state);
        }
        else if(CurrentState != MusicState.Light)
        {
            SetMusic(state);
            
        }
    }

    private void SetMusic(MusicState state)
    {
        string stateName = System.Enum.GetName(typeof(MusicState), state);
        AKRESULT result = AKRESULT.AK_Cancelled;
        int i = 0;
        do
        {
            if (i >= 4)
            {
                Debug.LogError("Set Music State Fail : " + result.ToString());
            }
            result = AkSoundEngine.SetState("Music", stateName);
            i++;

        } while (result != AKRESULT.AK_Success);
        m_currentState = state;
    }

    public void PlayFootSteps(GameObject from)
    {

        PlayEvent(SoundEvents.Play_Teto_Footsteps, from);
    }

    public void SetSwitch(Surfaces surface,GameObject from)
    {
        string surfaceName = System.Enum.GetName(typeof(Surfaces), surface);
        AkSoundEngine.SetSwitch("Surfaces", surfaceName, from);
    }

   public void ChangeBus(BusName bus, ref AkEnvironment env)
   {
        string busName = System.Enum.GetName(typeof(BusName), bus);
        int id =(int)AkSoundEngine.GetIDFromString(busName);
        env.SetAuxBusID(id);
    }

    public void StopAllEvents()
    {
        AkSoundEngine.StopAll();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        StopAllEvents();
    }


}
