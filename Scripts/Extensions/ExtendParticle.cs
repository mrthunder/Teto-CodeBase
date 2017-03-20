using UnityEngine;
using System.Collections;

public static class ExtendParticle {

    public static IEnumerator WaitUntilParticleComplete(this ParticleSystem p)
    {
        if(p.loop)
        {
            throw new System.Exception("The Particle is set to loop, it will never finish");
            
        }
        yield return new WaitUntil(() => !p.IsAlive());
    }
	
}
