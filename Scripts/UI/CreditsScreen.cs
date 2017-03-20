using UnityEngine;
using System.Collections;

public class CreditsScreen : UIScreen {

   
        public void OnBackButton()
        {
            ScreenManager.Instance.Show<MenuScreen>();
        }
  }
