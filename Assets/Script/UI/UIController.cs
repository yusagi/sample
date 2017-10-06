using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    private Animator m_Animator;
    private Image  m_Image;

    // Use this for initialization
    void Start () {
        m_Animator = GetComponent<Animator>();
        m_Image = GetComponent<Image>();
   }
	
	// Update is called once per frame
	void Update () {
		
	}

    //Activeの変更
    public void ChangeActive(bool active) {
        gameObject.SetActive(active);
    }

    //Animator切り替え
    public void ChangeAnimation(string name, bool flg = true) {
        m_Animator.SetBool(name, flg);
    }

    //Colorの切り替え
    public void ChangeImageColor(Color color) {
        m_Image.color = color;
    }
}
