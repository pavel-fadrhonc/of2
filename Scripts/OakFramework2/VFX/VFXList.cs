using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXList
{
	/// VFX_LIST_START

	public enum EVFX {
		NONE = 0,
		TESTVFX = 552121307,
	}

	public static string Get(EVFX vfx){
	string vfxId = null;
	switch(vfx){
		case EVFX.TESTVFX:
			vfxId = "TestVFX";
			break;
	}
	return vfxId;
	}
/// VFX_LIST_END
}
