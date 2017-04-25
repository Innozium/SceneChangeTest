using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//현재 씬이 로드되고 있는지 아닌지를 나타낸다.
public enum SCENESTATE
{
    NONE = 0,
    LOAD,
    END
}


public class SceneChangeManager : MonoBehaviour {

    //카메라 Fade In OUT을 하기 위한 스크립트를 저장할 변수들
    private CameraFadeInOut fadeInOutCtrl = null;
    public float FadeOutTime = 0.5f;
    public float FadeInTime = 0.5f;

    //현재 씬의 로드 상태를 알려줄 ENUM
    public SCENESTATE sceneState = SCENESTATE.NONE;

    //로딩 관련 정보가 들어갈 변수
    private AsyncOperation async = null;

    //로딩페이지를 별도 이미지로 할때 사용할 변수
    //로딩바를 가지고 있는 캔버스를 넣으면된다.
    //프리팹을 불러오도록 Init부분에 추가해서 사용하자.
    private GameObject LoadPagePrefeb;
    private Sprite loadProgressImg;
    private GameObject LoadPage;
    private Image LoadPageBg; //이미지를 넣으면 된다.
    private Image LoadPageProgressImgBg; //이미지를 넣으면 된다.
    private Image LoadPageProgressImg; //이미지를 넣으면 된다.

    private bool is_First = false;

    //싱글턴 패턴을 위한 인스턴스 변수 선언
    private static SceneChangeManager instance = null;

    public static SceneChangeManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gObj = new GameObject("_SceneChangeManager");
                instance = gObj.AddComponent<SceneChangeManager>();
            }

            return instance;
        }
    }

    private void Awake()
    {
        //SceneChangeManager 클래스를 인스턴스에 대입
        instance = this;

        //씬이 넘어가도 오브젝트가 삭제되지 않도록 한다.
        DontDestroyOnLoad(this.gameObject);

        //init을 통해 설정하자.
        Init();
    }


    //초기화 설정할 것들이 있다면 이곳에 추가하자.
    public void Init()
    {
        //오브젝트에 CameraFadeInOut 컴포넌트가 없다면 추가해준 뒤 변수에 할당한다.
        if (fadeInOutCtrl == null)
        {
            fadeInOutCtrl = GetComponent<CameraFadeInOut>();
            if (fadeInOutCtrl == null)
            {
                fadeInOutCtrl = this.gameObject.AddComponent<CameraFadeInOut>();
            }
        }

        LoadPagePrefeb = (GameObject)Resources.Load("LoadPage/Prefeb/LoadPageCanvas");
        LoadPage = (GameObject)Instantiate(LoadPagePrefeb);
        LoadPage.name = "LoadPage";
        LoadPage.SetActive(false);
        DontDestroyOnLoad(LoadPage);
        LoadPageBg = LoadPage.transform.FindChild("LoadImage").GetComponent<Image>();
        LoadPageBg.sprite = Resources.Load<Sprite>("LoadPage/Image/LoadPageBg");
        LoadPageProgressImgBg = LoadPage.transform.FindChild("LoadBarBg").GetComponent<Image>();
        LoadPageProgressImgBg.sprite = Resources.Load<Sprite>("LoadPage/Image/LoadBarBg");
        LoadPageProgressImgBg.color = new Color(LoadPageProgressImgBg.color.r, LoadPageProgressImgBg.color.g, LoadPageProgressImgBg.color.b, 0.3f);
        LoadPageProgressImgBg.type = Image.Type.Filled;
        LoadPageProgressImg = LoadPage.transform.FindChild("LoadBar").GetComponent<Image>();
        LoadPageProgressImg.sprite = Resources.Load<Sprite>("LoadPage/Image/LoadBar");
        LoadPageProgressImg.type = Image.Type.Filled;
        LoadPageProgressImg.fillMethod = Image.FillMethod.Horizontal;
        LoadPageProgressImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        LoadPageProgressImg.fillAmount = 0.0f;
    }


    // Update is called once per frame
    void Update () {

        ////테스트용
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    //fadeInOutCtrl.FadeIn(FadeInTime);
        //    OnSceneChange("ScTest001");
        //}
        //else if (Input.GetKeyDown(KeyCode.S))
        //{
        //    //fadeInOutCtrl.FadeOut(FadeOutTime);
        //    OnSceneChange("ScTest002");
        //}

    }

    //씬을 바꾸기 위해 외부에서 호출하는 함수
    public void OnSceneChange(string name)
    {
        if (sceneState == SCENESTATE.LOAD) return;

        //CoLoadGame 코루틴을 실행한다.
        StartCoroutine(CoLoadGame(name));
    }

    //외부에서 FadeIn을 요구할 때 사용할 함수
    //이렇게 사용 하는 이유
    //외부에서 www를 통해 이미지 등을 불러오는 경우에는
    //페이지 로딩이 끝나도 이미지 로딩이 끝나지 않아 있다.
    //그렇기 때문에 모든 로딩이 끝나고 이 함수를 호출하여 사용하자.
    public void OnFadeIN()
    {
        StartCoroutine(CoFadeIN());
    }

    //OnFadeIN 함수에서 호출할 코루틴
    IEnumerator CoFadeIN()
    {
        //로딩이 끝났으니 END로.
        sceneState = SCENESTATE.END;

        if (LoadPageProgressImg != null && LoadPage != null)
        {//별도의 이미지를 사용중이라면 마지막 갱신 뒤 비활성화해주자.
         //페이드인 따로 추가할까?

            //카메라 페이드 아웃
            this.fadeInOutCtrl.FadeOut(FadeOutTime);
            yield return new WaitForSeconds(FadeOutTime);
            LoadPage.SetActive(false);
        }

        // 로드가 끝난후 페이드 인 들어가게 바꾼다.
        this.fadeInOutCtrl.FadeIn(this.FadeInTime);
    }

    //CoLoadGame에서 사용하는 함수
    //프로젝트에서는 주석처리하고 OnFadeIn을 사용 중
    //www로 로딩하는 외부 리소스가 없다면 주석 해제하고 이것을 사용하자.
    IEnumerator CoDoneCheck()
    {
        //씬이 완전히 넘어갔는지 무한 체크!
        while (!async.isDone)
        {
            yield return null;
        }

        Debug.Log("넘어간뒤" + async.isDone);
        if (async.isDone)
        {
            sceneState = SCENESTATE.END;
            if(LoadPageProgressImg != null && LoadPage != null)
            {//별도의 이미지를 사용중이라면 마지막 갱신 뒤 비활성화해주자.
                //카메라 페이드 아웃
                this.fadeInOutCtrl.FadeOut(this.FadeOutTime);
                yield return new WaitForSeconds(this.FadeOutTime);
                LoadPage.SetActive(false);
            }
        }

        // 로드가 끝난후 페이드 인 들어가게 바꾼다.
        this.fadeInOutCtrl.FadeIn(this.FadeInTime);
    }

    //OnSceneChange에서 실행시키는 코루틴 함수
    //씬 로딩에 대해 실행된다.
    IEnumerator CoLoadGame(string sceneName)
    {

        //현재 로드상태를 설정한다.
        sceneState = SCENESTATE.LOAD;

        //카메라 페이드 아웃
        this.fadeInOutCtrl.FadeOut(this.FadeOutTime);

        //카메라가 페이드 아웃 될 시간동안 실행을 미룬다.
        yield return new WaitForSeconds(this.FadeOutTime);

       
        //로딩 페이지를 활성화 시킨다.
        if (LoadPage != null)
        {
            LoadPage.SetActive(true);
            if (LoadPageProgressImg != null) LoadPageProgressImg.fillAmount = 0.0f;
            // 로드페이지가 보여주기 위해 페이드 인!!
            this.fadeInOutCtrl.FadeIn(this.FadeInTime);
            yield return new WaitForSeconds(this.FadeInTime);
            //print("ss");
            yield return null;
        }

        //비동기 로딩을 하기 위한 변수
        async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        //장면이 준비된 즉시 장면이 활성화되는 것을 막는다.
        async.allowSceneActivation = false;

        //로딩이 완료되었는지 확일할 불 변수
        bool isDone = false;

        while (!isDone)
        {
            //퍼센트 표시를 하고 싶다면 이것을 사용하자!
            //layer.text (async.progress * 111.111f + 0.00011f).ToString() + "%";
            
            //로딩바를 표시할 이미지가 있다면 갱신한다.
            if (LoadPageProgressImg != null) LoadPageProgressImg.fillAmount = async.progress;

            yield return null;
            //로딩이 90% 이상이면 탈출한다.
            if (async.progress >= 0.9f)
                isDone = true;

            //Debug.Log((async.progress * 111.111f + 0.00011f).ToString() + "%");
        }

        //로딩바를 한번 더 갱신한다.
        if(LoadPageProgressImg != null) LoadPageProgressImg.fillAmount = async.progress;

        //장면이 준비된 즉시 장면이 활성화되는 것을 허용한다.
        async.allowSceneActivation = true;

        //로드가 끝났는지 체크할 코루틴을 실행한다.
        StartCoroutine(CoDoneCheck());
    }
}
