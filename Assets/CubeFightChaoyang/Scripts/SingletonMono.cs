//****************************************************
//    文件名（File Name）:   NewBehaviourScript.cs
//    作者（Author）:    小时候特别帅
//   
//    创建时间（CreateTime）:  2016-12-12 18:27:49
//
//    脚本挂载位置 :    #
//
//    脚本功能:    #
//     
//****************************************************

using UnityEngine;
using System.Collections;
public class SingleTonMono<T> : MonoBehaviour where T : MonoBehaviour {

    static T instance;
    public static T Instance {
        get {
            if (instance == null) {
                //instance = new T();
                GameObject go = new GameObject();
                //go.name = typeof(T).GetType().ToString();
                go.name = typeof(T).ToString();

                instance = go.AddComponent<T>();
            }
            return instance;
        }
    }
    protected virtual void Awake() {
        instance = this as T;//父类 as 子类  两个类都继承了Mono
    }
}
