using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 
public class CharacterSelector : MonoBehaviour
{

private GameObject[] characterList;
private int index;
private void Start()
{
    index = PlayerPrefs.GetInt("CharacterSelected");
	characterList = new GameObject[transform.childCount];
	
//Fill array with our models
for(int i=0 ; i< transform.childCount ; i++)
{
	characterList[i] = transform.GetChild(i).gameObject;
}
//we toggle off their renderer
foreach (GameObject go in characterList)
	go.SetActive(false);


//we toggle on the first index
if(characterList[0])
characterList[0].SetActive(true);



}




public void ToggleLeft(){

characterList[index].SetActive(false);
index--;
if(index < 0)
index = characterList.Length - 1;

characterList[index].SetActive(true);
}

public void ToggleRight(){

characterList[index].SetActive(false);
index++;
if(index == characterList.Length)
index = 0;

characterList[index].SetActive(true);
}

    public void Comfirm(){
        PlayerPrefs.SetInt("CharacterSelected", index);
        SceneManager.LoadScene("SampleScene");


    }


}




