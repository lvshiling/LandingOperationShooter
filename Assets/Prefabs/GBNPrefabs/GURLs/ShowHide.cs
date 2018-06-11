using UnityEngine;

namespace GURLs
{
    public class ShowHide : MonoBehaviour {
	
        public GameObject[] menu;
        public GameObject btn1;
        public GameObject btn2;

        private bool b;

        private void Update () {
            b = false;
            foreach (var go in menu) {
                if (go != null && go.activeSelf)
                {
                    b = true;
                }
            }
		
            if (b) {
                if (btn1 != null && !btn1.activeSelf)
                    btn1.SetActive(true);
                if (btn2 != null && !btn2.activeSelf)
                    btn2.SetActive(true);
            } else {
                if (btn1 != null && btn1.activeSelf)
                    btn1.SetActive (false);
                if (btn2 != null && btn2.activeSelf)
                    btn2.SetActive (false);
            }
        }
    }
}