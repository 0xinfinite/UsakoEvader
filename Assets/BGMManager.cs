using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Audio;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [SerializeField] double loopStartTime;
    [SerializeField] double loopEndTimeOffset;
    //[SerializeField] double firstLoopEndTimeOffset;
  //  [SerializeField] GameObject obj;

    //[SerializeField] BGMPlayables bgm;
    //[SerializeField] double speed=1;
    //[SerializeField] float volume=1;
    [SerializeField] double loopReadyTime=0.2;
    
    public AudioClip music;

    PlayableGraph graph;

   // bool isOverlaped;

    public AudioSource myAudio;

    // Start is called before the first frame update
    void Awake()
    {
        if (!myAudio)
            myAudio = GetComponent<AudioSource>();

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //if (music != instance.music)
            //{
            //    instance.music = music;
            //}
            if (myAudio != null)
            {
                if(myAudio.clip != null)
                {
                    if (myAudio.clip != instance.myAudio.clip)
                    {
                        Destroy(instance.gameObject);
                        instance = this;
                        DontDestroyOnLoad(this.gameObject);
                        return;
                        //ChangeClip(myAudio.clip);
                    }
                }
                
            }
           
        //    isOverlaped = true;
            Destroy(this.gameObject);
            return;
        }


        // //bgm = new BGMPlayables();
        //// bgm.audioClip = music;

        // graph = PlayableGraph.Create();

        // bgm.CreatePlayable(graph, obj);

        // //        bgm.audioPlayable1.Play();

        // //graph.Play();

        // bgm.Init(loopStartTime, speed);
    }

    bool isInit;

    private void Start()
    {
    //     bgm.audioPlayable1.SetTime(bgm.GetDuration(true) - loopEndTimeOffset - loopReadyTime -0.1);     //처음 루프가 어긋나는 문제의 긴급픽스, 처음 3초동안 bgm 재생이 안됨(즉각 재생 불가)
    //        graph.Play();

    //    changeVolumeDuration = -1;
    }

    public void ControlVolume(float _targetVolume, float _duration)
    {
        changeVolumeTime = 0;
        prevVolume = myAudio.volume;
        targetVolume = _targetVolume;
        changeVolumeDuration = _duration;
        mutiplyProcessByTime = 1 / changeVolumeDuration;
    }
    float prevVolume;
    float targetVolume;
    float changeVolumeDuration;
    float changeVolumeTime;
    float mutiplyProcessByTime;

    public void ChangeClip(AudioClip clip)
    {
        myAudio.clip = clip;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isInit)
        //{
        //    isInit = true;
        //    bgm.audioPlayable1.SetTime(bgm.GetDuration(true) - loopEndTimeOffset - 3);
        //    graph.Play();
        //}
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            myAudio.time = myAudio.clip.length - (float)loopEndTimeOffset - (float)loopReadyTime - 1f;
            //    if(bgm.IsCurrentPlayingInput0())
            //    bgm.audioPlayable1.SetTime(bgm.GetDuration(true)-loopEndTimeOffset-loopReadyTime -0.1);
            //    else
            //    {
            //        bgm.audioPlayable2.SetTime(bgm.GetDuration(false) - loopEndTimeOffset - loopReadyTime - 0.1);
            //    }
        }
#endif

        if (changeVolumeDuration > changeVolumeTime)
        {
            myAudio.volume = Mathf.Lerp(prevVolume, targetVolume, changeVolumeTime * mutiplyProcessByTime);
            changeVolumeTime += Time.deltaTime;
        }

        //bgm.LoopBGM(loopStartTime, loopEndTimeOffset, loopReadyTime, volume);

        if (myAudio.time > myAudio.clip.length - loopEndTimeOffset) {
            myAudio.time = (float)loopStartTime;
        };
    }

    //private void OnDestroy()
  //  {
     //   if(!isOverlaped)
     //   graph.Destroy();
   // }

}

////public struct BGMPlayables{//
//[CreateAssetMenu(menuName = "Audio/BGM Playables Assets")]
//public class BGMPlayables : PlayableAsset { 

//        public AudioClip audioClip;

//        public AudioClipPlayable audioPlayable1;
//        public AudioClipPlayable audioPlayable2;
//        public AudioMixerPlayable audioMixer;

//    // public override double duration { get { return kClipDuration * 2; } }
    
//    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
//    {
//        Debug.Log("Create Playable");

//        audioPlayable1 = AudioClipPlayable.Create(graph, audioClip, true);
//             audioPlayable2 = AudioClipPlayable.Create(graph, audioClip, true);
//        audioMixer = AudioMixerPlayable.Create(graph, 2);

//        graph.Connect(audioPlayable1, 0, audioMixer, 0);
//            graph.Connect(audioPlayable2, 0, audioMixer, 1);

//        //  audioPlayable1.Seek(0, 0, kClipDuration);
//        // audioPlayable2.Seek(0, kBlendStart, kClipDuration);
//        audioMixer.SetInputWeight(0, 0);// 1);      처음에 재생시점을 일부로 뒤로 땡겨놓은걸 숨기기 위해 input을 끊어놓음
//          audioMixer.SetInputWeight(1, 0);

//        var output = AudioPlayableOutput.Create(graph, string.Empty, null);
//        output.SetSourcePlayable(audioMixer);
//        isInit = true;
//        return audioMixer;
//    }

//    public void Init(double loopStartTime, double speed)
//    {
//        isPlayingInput0 = true;
//        //isInit = true;
//        //audioPlayable2.Pause();
//        audioPlayable2.SetTime(loopStartTime);

//        audioPlayable1.SetSpeed(speed);
//        audioPlayable2.SetSpeed(speed);

//    }

//    //    public void Configure()
//    //{
//    //    PlayableGraph graph = PlayableGraph.Create();


//    //}
//    bool isPlayingInput0;
//    public bool IsCurrentPlayingInput0()
//    {
//        return isPlayingInput0;
//    }

//    public double GetDuration(bool is1)
//    {
//        if (is1)
//            return audioPlayable1.GetDuration();

//        return audioPlayable2.GetDuration();
//    }

//    bool isInit;
//    bool isReady;

//    public void ChangeBGM(AudioClip clipToChange, double loopEndTimeOffset, double loopReadyTime)
//    {
//        isPlayingInput0 = true;
//        isInit = true;
//        audioMixer.SetInputWeight(0, 0);
//        audioMixer.SetInputWeight(1, 0);
//        audioPlayable1.SetTime(audioPlayable1.GetDuration() - loopEndTimeOffset - loopReadyTime - 0.1);
//        audioPlayable1.SetClip(clipToChange);
//        audioPlayable2.SetClip(clipToChange);
//    }

//    public void LoopBGM(double loopStartTime, double loopEndTimeOffset, double loopReadyTime, float volume)
//    {
        

//        if (isPlayingInput0)
//        {
//            if(!isInit)
//            audioMixer.SetInputWeight(0, volume);
//            if (audioPlayable1.GetTime() >= audioPlayable1.GetDuration() - loopEndTimeOffset- loopReadyTime && !isReady)
//            {
//                audioPlayable2.SetTime(loopStartTime - loopReadyTime);
//                if(audioPlayable2.GetPlayState() != PlayState.Playing )
//                audioPlayable2.Play();

//                isReady = true;
//            }

//                if (audioPlayable1.GetTime() >= audioPlayable1.GetDuration() - loopEndTimeOffset)
//            {
//                if (isInit)
//                    audioPlayable2.SetTime(0);

//                audioMixer.SetInputWeight(0, 0);
//                audioMixer.SetInputWeight(1, volume);
//                isReady = false;
//                isPlayingInput0 = false;
//                isInit = false;
//            }
//        }
//        else
//        {
//            audioMixer.SetInputWeight(1, volume);
//            if (audioPlayable2.GetTime() >= audioPlayable2.GetDuration() - loopEndTimeOffset - loopReadyTime && !isReady)
//            {
//                audioPlayable1.SetTime(loopStartTime - loopReadyTime);
//                if (audioPlayable1.GetPlayState() != PlayState.Playing)
//                    audioPlayable1.Play();

//                isReady = true;
//            }
//            if (audioPlayable2.GetTime() >= audioPlayable2.GetDuration() - loopEndTimeOffset)
//            {
                
//                audioMixer.SetInputWeight(0, volume);
//                audioMixer.SetInputWeight(1, 0);
//                isReady = false;
//                isPlayingInput0 = true;
//            }
//        }
        
//    }

   
//}
