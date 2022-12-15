using UnityEngine;

[AddComponentMenu("NGUI/Examples/Load Level On Click")]
public class LoadLevelOnClick : MonoBehaviour
{
	public string levelName;


    public void PublicOnClick()
    {
        //levelName = _name;
        OnClick();
    }




	void OnClick ()
	{
		Time.timeScale = 1.0f;


		if (!string.IsNullOrEmpty(levelName))
		{
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			Application.LoadLevel(levelName);
#else
			UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
#endif
		}
	}
}
