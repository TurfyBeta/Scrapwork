using UnityEngine;

public class DSHandler : MonoBehaviour
{
    public MeshRenderer[] screenList;
    public Material[] matList;
    private float[] currVal;
    private float[] targetVal;
    private float[] smoothAmount;
    private bool camState = false;
    private bool lastCamState = false;

    void Start(){
        targetVal = new float[]{0.03f, 0.02f, 3f};
        currVal = new float[]{0.03f, 0.02f, 3f};
        smoothAmount = new float[]{220f, 220f, 220f};


        for (int i = 0; i < screenList.Length; i++)
            screenList[i].material = matList[i];
    }

    void Update(){
        bool needUpdate = lastCamState != camState;
        for (int i = 0; i < targetVal.Length; i++) {
            if (Mathf.Abs(targetVal[i] - currVal[i]) > 0.01f) {
                currVal[i] += (targetVal[i] - currVal[i]) / smoothAmount[i];
                needUpdate = true;
            }
        }
        if (needUpdate) UpdateMats();
        lastCamState = camState;
    }

    void UpdateMats(){
        for (int i = 0; i < matList.Length; i++) {
            matList[i].SetFloat("_NoiseAmount", currVal[0]);
            matList[i].SetFloat("_AbberationAmount", currVal[1]);
            matList[i].SetFloat("_TextureShift", currVal[2]);
            matList[i].SetInt("_ScreenOn", (camState) ? 1 : 0);
        }
    }

    System.Collections.IEnumerator ActivateCams() {
        yield return new WaitForSeconds(2f);
        camState = true;
        FlashBang();
    }

    public void CamsState(bool isOn) {
        if (isOn && camState == false) {
            StartCoroutine(ActivateCams());
        } else if (!isOn && camState == true) {
            camState = false;
        }
    }

    public void FlashBang() {
            currVal[0] = 1f;
            currVal[1] = 0f;
            currVal[2] = 25f;
    }
}
