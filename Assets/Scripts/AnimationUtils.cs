using DragonBones;
using UnityEngine;

public static class AnimationUtils
{
    public static bool GetCurrentAnimationEnded(Animator animator, int currentAnimHash)
    {
        AnimatorStateInfo animatorInfo = animator.GetCurrentAnimatorStateInfo(0);

        return animatorInfo.normalizedTime >= 1f && animatorInfo.shortNameHash == currentAnimHash;
    }

    public static float GetCurrentAnimationNormalizedTime(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public static float GetCurrentAnimationLength(Animator animator)
    {
        return animator.GetCurrentAnimatorClipInfo(0).Length;
    }

    public static bool GetCurrentAnimationEnded(UnityArmatureComponent armature)
    {
        DragonBones.AnimationState animState = armature.animation.lastAnimationState;

        float normalizedTime = animState.currentTime / animState.totalTime;

        return normalizedTime >= 1f; //&& animatorInfo.shortNameHash == currentAnimHash;
    }

    public static bool GetAnimationEnded(UnityArmatureComponent armature, string name)
    {
        DragonBones.AnimationState animState = armature.animation.GetState(name);

        return animState.isCompleted;
    }

    public static float GetCurrentAnimationNormalizedTime(UnityArmatureComponent armature)
    {
        DragonBones.AnimationState animState = armature.animation.lastAnimationState;

        float normalizedTime = animState.currentTime / animState.totalTime;

        return normalizedTime;
    }

    public static float GetCurrentAnimationLength(UnityArmatureComponent armature)
    {
        DragonBones.AnimationState animState = armature.animation.lastAnimationState;
        return animState.totalTime;
    }

    //public static void AccelerateCurrentAnimation(Animator animator, float duration)
    //{
    //    float animLength = GetCurrentAnimationLength();
    //    float currentNormTime = GetCurrentAnimatioNNormalizedTime();
    //
    //    float remainingTime = animLength - animLength * currentNormTime;
    //
    //    if (remainingTime > duration)
    //    {
    //        animator.SetFloat(animSpeedHash, remainingTime * (1f / duration));
    //    }
    //}
}
