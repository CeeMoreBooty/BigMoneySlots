using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Utility for loading and caching profile images from URLs.
/// Prevents re-downloading images and provides default avatar fallback.
/// </summary>
public class ImageCache : MonoBehaviour
{
    public static ImageCache Instance { get; private set; }

    [Header("Default Avatar")]
    [SerializeField] private Sprite defaultAvatar;

    private Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
    private Dictionary<string, List<Action<Texture2D>>> _pendingCallbacks = new Dictionary<string, List<Action<Texture2D>>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Load an image from URL and apply to Image component.
    /// Uses cache if available, downloads if not.
    /// </summary>
    public void LoadImageToUI(string url, Image targetImage, bool useDefaultOnEmpty = true)
    {
        if (targetImage == null) return;

        if (string.IsNullOrEmpty(url))
        {
            if (useDefaultOnEmpty && defaultAvatar != null)
                targetImage.sprite = defaultAvatar;
            return;
        }

        LoadImage(url, texture =>
        {
            if (texture != null && targetImage != null)
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                targetImage.sprite = sprite;
            }
            else if (useDefaultOnEmpty && defaultAvatar != null && targetImage != null)
            {
                targetImage.sprite = defaultAvatar;
            }
        });
    }

    /// <summary>
    /// Load an image from URL with callback.
    /// Uses cache if available, downloads if not.
    /// </summary>
    public void LoadImage(string url, Action<Texture2D> onComplete)
    {
        if (string.IsNullOrEmpty(url))
        {
            onComplete?.Invoke(null);
            return;
        }

        // Check cache
        if (_cache.TryGetValue(url, out Texture2D cachedTexture))
        {
            onComplete?.Invoke(cachedTexture);
            return;
        }

        // Check if already downloading
        if (_pendingCallbacks.ContainsKey(url))
        {
            _pendingCallbacks[url].Add(onComplete);
            return;
        }

        // Start download
        _pendingCallbacks[url] = new List<Action<Texture2D>> { onComplete };
        StartCoroutine(DownloadImage(url));
    }

    private IEnumerator DownloadImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            Texture2D texture = null;

            if (request.result == UnityWebRequest.Result.Success)
            {
                texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    _cache[url] = texture;
                }
            }
            else
            {
                Debug.LogWarning($"Failed to download image from {url}: {request.error}");
            }

            // Invoke all pending callbacks
            if (_pendingCallbacks.TryGetValue(url, out List<Action<Texture2D>> callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback?.Invoke(texture);
                }
                _pendingCallbacks.Remove(url);
            }
        }
    }

    /// <summary>
    /// Clear all cached images to free memory.
    /// </summary>
    public void ClearCache()
    {
        foreach (var texture in _cache.Values)
        {
            if (texture != null)
                Destroy(texture);
        }
        _cache.Clear();
    }

    /// <summary>
    /// Remove a specific image from cache.
    /// </summary>
    public void RemoveFromCache(string url)
    {
        if (_cache.TryGetValue(url, out Texture2D texture))
        {
            if (texture != null)
                Destroy(texture);
            _cache.Remove(url);
        }
    }

    /// <summary>
    /// Get the default avatar sprite.
    /// </summary>
    public Sprite GetDefaultAvatar()
    {
        return defaultAvatar;
    }

    /// <summary>
    /// Set a new default avatar sprite.
    /// </summary>
    public void SetDefaultAvatar(Sprite sprite)
    {
        defaultAvatar = sprite;
    }
}
