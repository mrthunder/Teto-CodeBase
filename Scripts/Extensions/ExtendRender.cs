using UnityEngine;
using System.Collections;

public static class ExtendRender {

    /// <summary>
    /// Make objects show or disappear based on the material
    /// </summary>
    public static void EnviromentTransition(this Renderer render,bool show)
    {
        
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            render.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat("_Show", (show ? 1 : 0));
            render.SetPropertyBlock(propertyBlock);

        
    }

}
