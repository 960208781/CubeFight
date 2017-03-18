//****************************************************
//    文件名（File Name）:   Singleton.cs
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

public abstract class Singleton<T> where T : new() {
    static T instance;
    public static T Instance {
        get {
            if (instance==null) {
                instance = new T();
            }
            return instance;
        }
    }
	

}
