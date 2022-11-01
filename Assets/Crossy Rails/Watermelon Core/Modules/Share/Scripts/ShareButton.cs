using UnityEngine;

namespace Watermelon
{
    public class ShareButton : MonoBehaviour
    {
        private void Awake()
        {
            if(!ShareController.Settings.isShareEnabled)
                gameObject.SetActive(false);
        }

        public void ShareDefaultMessage()
        {
            ShareController.ShareMessage();
        }
    }
}