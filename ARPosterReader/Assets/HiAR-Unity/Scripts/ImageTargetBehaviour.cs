﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using hiscene;
using Models;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(HiARBaseObjectMovement))]
public class ImageTargetBehaviour : ImageTarget, ITrackableEventHandler, ILoadBundleEventHandler 
{
	private bool targetFound = false;

    Text timeText;
    Text addressText;
    Text linkText;
    Text addressURL;
    Text keyGroup;
    Text keyId;
    Text posterTitle;
    CameraManager eventManager;
    ObserveImageTarget zoomBtn;
    UpdateFavouriteButton favouriteButton;

    private void Start()
    {
        posterTitle = GameObject.Find("PosterTitle").GetComponent<Text>();
        timeText = GameObject.Find("Time").GetComponent<Text>();
        addressText = GameObject.Find("Address").GetComponent<Text>();
        linkText = GameObject.Find("Web Link").GetComponent<Text>();
        addressURL = GameObject.Find("Address Url").GetComponent<Text>();
        keyGroup = GameObject.Find("KeyGroup").GetComponent<Text>();
        keyId = GameObject.Find("KeyId").GetComponent<Text>();
        eventManager = GameObject.Find("EventSystem").GetComponent<CameraManager>();
        zoomBtn = GameObject.Find("Zoom Button").GetComponent<ObserveImageTarget>();
        favouriteButton = GameObject.Find("Favourite").GetComponent<UpdateFavouriteButton>();
        zoomBtn.gameObject.SetActive(false);
        favouriteButton.gameObject.SetActive(false);

        if (Application.isPlaying)
        {
            for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(false);
        }
        else
        {
            for (var i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(true);
        }
        RegisterTrackableEventHandler(this);
        RegisterILoadBundleEventHandler(this);
    }

    public void OnLoadBundleStart(string url)
    {
        Debug.Log("load bundle start: " + url);
    }

    public void OnLoadBundleProgress(float progress)
    {
        Debug.Log("load bundle progress: " + progress);
    }

    public void OnLoadBundleComplete() { }

    public virtual void OnTargetFound(RecoResult recoResult)
    {
        if (recoResult.IsCloudReco)
        {
            downloadBundleFromHiAR(recoResult);
            recoResult.KeyGroup = "ARPosterSample";
        }
        for (var i = 0; i < transform.childCount; i++)
        {
            //transform.GetChild(i).rotation = Quaternion.Euler(-90, 0, 0);
            transform.GetChild(i).gameObject.SetActive(true);
            
        }
		targetFound = true;
        zoomBtn.imageTargeter = this.gameObject;
        zoomBtn.UpdateTargetBehaviour();
        eventManager.aimImageTarget = this.gameObject;

        // The Method in Loom.Run Async can start a thread. And In the Thread, add the action that can only process on main thread.
        Loom.RunAsync(() => {
            Poster poster = new Poster();
            poster.keygroup = recoResult.KeyGroup;
            poster.keyid = recoResult.KeyId;
            Thread thread = new Thread(new ParameterizedThreadStart(getPoster));
            thread.Start(poster);
        });
    }

    public void getPoster(object obj)
    {
        Poster poster = obj as Poster;
        poster.GetPoster();

        //The action added to Loom.QueueOnMainThread is run on Main Thread.
        Loom.QueueOnMainThread(showDetail, poster);
    }

    public void showDetail(object detail)
    {
        Poster detailPos = detail as Poster;

        posterTitle.text = detailPos.postitle;
        posterTitle.name = detailPos.postitle;
        timeText.text = detailPos.posdate;
        addressText.text = detailPos.poslocation;
        linkText.text = detailPos.poslink;
        addressURL.text = detailPos.posmap;
        keyGroup.text = detailPos.keygroup;
        keyId.text = detailPos.keyid;
        
        //GameObject favouriteButton = GameObject.Find("Favourite");
        if (favouriteButton != null && storeLoginSessionId.loginId!=-1)
        {
            //favouriteButton.gameObject.SetActive(true);
            favouriteButton.GetComponent<UpdateFavouriteButton>().changeText();
        }
    }

    public void clearDetail()
    {
        posterTitle.text = "Title";
        posterTitle.name = "";
        timeText.text = "Time";
        addressText.text = "Address";
        linkText.text = "Web Link";
        addressURL.text = "";
        keyGroup.text = "";
        keyGroup.name = "";

        //GameObject favouriteButton = GameObject.Find("Favourite");
        if (favouriteButton != null)
        {
            //favouriteButton.gameObject.SetActive(true);
            favouriteButton.GetComponent<UpdateFavouriteButton>().changeText();
            //favouriteButton.gameObject.SetActive(true);
        }
    }

    public virtual void OnTargetTracked(RecoResult recoResult, Matrix4x4 pose) { }

    public virtual void OnTargetLost(RecoResult recoResult)
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
		targetFound = false;
        zoomBtn.UpdateTargetBehaviour();
        clearDetail();
    }

	public bool IsTargetFound()
	{
		return targetFound;
	}

    public void OnLoadBundleError(Exception error)
    {
        Debug.Log("load bundle error: " + error.ToString());
    }

    public override void ConfigCloudObject(IHsGameObject target)
    {
        try
        {//兼容老版本识别包
            HiARObjectMonoBehaviour oldScript = target.HsGameObjectInstance.GetComponent<HiARObjectMonoBehaviour>();
            if (oldScript != null && target.HsGameObjectInstance.transform.childCount > 0)
            {
                GameObject child = target.HsGameObjectInstance.transform.GetChild(0).gameObject;
                VideoPlayerMonoBehaviour oldVideo = child.GetComponent<VideoPlayerMonoBehaviour>();
                if (oldVideo != null)
                {
                    child.AddComponent<VideoPlayerBehaviour>();
                    VideoPlayerBehaviour player = child.GetComponent<VideoPlayerBehaviour>();
                    player.m_isLocal = false;
                    player.m_webUrl = oldVideo.m_webUrl;
                    if (string.IsNullOrEmpty(player.m_webUrl))
                    {
                        player.m_isLocal = true;
                        player.m_localPath = oldVideo.m_localPath;
                    }
                }
                target.HsGameObjectInstance = child;
            }
        }
        catch (Exception e)
        {
            LogUtil.Log(e.ToString());
        }

        VideoPlayerBehaviour playerSrc = target.HsGameObjectInstance.GetComponent<VideoPlayerBehaviour>();
        if (playerSrc != null)
        {
            target.HsGameObjectInstance.name = "VideoPlayer";
            VideoPlayer.TransParentOptions option = playerSrc.TransParentOption;
            Material material = Resources.Load<Material>("Materials/VIDEO");
            switch (option)
            {
                case VideoPlayer.TransParentOptions.None:
                    if (playerSrc.IsTransparent)
                    {
                        material = Instantiate(Resources.Load<Material>("Materials/VIDEO"));
                        material.shader = Instantiate(Resources.Load<Shader>("Shaders/Transparent_Color"));
                    }
                    else
                    {
                        material.shader = Resources.Load<Shader>("Shaders/video");
                    }
                    break;
                case VideoPlayer.TransParentOptions.TransparentColor:
                    material = Instantiate(Resources.Load<Material>("Materials/VIDEO"));
                    material.shader = Instantiate(Resources.Load<Shader>("Shaders/Transparent_Color"));
                    break;
                case VideoPlayer.TransParentOptions.TransparentLeftAndRight:
                    material.shader = Resources.Load<Shader>("Shaders/TransparentVideo_LeftAndRight");
                    break;
                case VideoPlayer.TransParentOptions.TransparentUpAndDown:
                    material.shader = Resources.Load<Shader>("Shaders/TransparentVideo_UpAndDown");
                    break;
                default:
                    break;
            }
            playerSrc.PlayMaterial = material;
            if(playerSrc.IsTransparent || (playerSrc.TransParentOption == VideoPlayer.TransParentOptions.TransparentColor))
            {
                playerSrc.PlayMaterial.SetFloat("_DeltaColor", playerSrc.DeltaColor);
                playerSrc.PlayMaterial.SetColor("_MaskColor", playerSrc.MaskColor);
            }
        }

        Transform trans = target.HsGameObjectInstance.transform;
        //Vector3 scale = trans.localScale;
        Vector3 position = trans.position;
        Quaternion rotation = trans.rotation;

        trans.position = transform.position;
        trans.rotation = transform.rotation;

        trans.SetParent(transform);

        trans.localPosition = position;
        trans.localRotation = rotation;

        trans.gameObject.SetActive(true);
    }
}
