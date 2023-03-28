using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Errot_Test_CPP : MonoBehaviour
{

    //  [SerializeField] private Text _label;
    [SerializeField] private List<GameObject> _il2cppButtons;


    // Start is called before the first frame update
    void Start()
    {
#if !ENABLE_IL2CPP
        // _label.color = Color.red;
        foreach (var il2CPPButton in _il2cppButtons)
        {
            il2CPPButton.GetComponent<Button>().interactable = false;
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ThrowCpp() => throw_cpp();
    public void CrashInCpp() => crash_in_cpp();

    // CppPlugin.cpp
    [DllImport("__Internal")]
    private static extern void throw_cpp();
    [DllImport("__Internal")]
    private static extern void crash_in_cpp();
}
