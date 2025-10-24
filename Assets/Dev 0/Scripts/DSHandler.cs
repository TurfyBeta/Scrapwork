using UnityEngine;

public class DSHandler : MonoBehaviour
{
    public MeshRenderer[] screenList;
    public Material[] matList;
    private float[] currVal;
    private float[] targetVal;
    private float[] smoothAmount;

    void Start(){
        targetVal = new float[]{0.05f, 0.02f, 5f};
        currVal = new float[]{0.05f, 0.02f, 5f};
        smoothAmount = new float[]{20f, 120f, 20f};


        for (int i = 0; i < screenList.Length; i++)
            screenList[i].material = matList[i];
    }

    void Update(){

        if (Input.GetKeyDown(KeyCode.P))
        {
            currVal[0] = 1f;
            currVal[1] = 0f;
            currVal[2] = 5f;
        }
        bool needUpdate = false;
        for (int i = 0; i < targetVal.Length; i++) {
            if (Mathf.Abs(targetVal[i] - currVal[i]) > 0.01f) {
                currVal[i] += (targetVal[i] - currVal[i]) / smoothAmount[i];
                needUpdate = true;
            }
        }
        if (needUpdate) UpdateMats();
    }

    void UpdateMats(){
        for (int i = 0; i < matList.Length; i++) {
            matList[i].SetFloat("_NoiseAmount", currVal[0]);
            matList[i].SetFloat("_AbberationAmount", currVal[1]);
            matList[i].SetFloat("_TextureShift", currVal[2]);
        }
    }
}
