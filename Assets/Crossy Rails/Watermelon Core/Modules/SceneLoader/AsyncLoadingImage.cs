using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Image))]
    public class AsyncLoadingImage : MonoBehaviour
    {
        private Image loadingImage;

        private void Awake()
        {
            loadingImage = GetComponent<Image>();
            loadingImage.fillAmount = 0;
        }

        private void OnEnable()
        {
            SceneLoader.onAsyncLoadProgressChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            SceneLoader.onAsyncLoadProgressChanged -= OnStateChanged;
        }

        private void OnStateChanged(float progress)
        {
            loadingImage.fillAmount = progress;
        }
    }
}