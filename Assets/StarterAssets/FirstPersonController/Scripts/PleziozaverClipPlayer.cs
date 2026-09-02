using UnityEngine;

/// <summary>
/// Plays the plesiosaur's imported bone animation without requiring a Humanoid/Generic Avatar.
/// This is useful for Blender creature rigs that import valid transform curves but no Avatar asset.
/// </summary>
public sealed class PleziozaverClipPlayer : MonoBehaviour
{
    [SerializeField] private AnimationClip animationClip;
    [SerializeField, Min(0f)] private float playbackSpeed = 1f;

    private float playbackTime;

    public AnimationClip AnimationClip
    {
        get => animationClip;
        set => animationClip = value;
    }

    private void OnEnable()
    {
        playbackTime = 0f;
        SampleCurrentFrame();
    }

    private void LateUpdate()
    {
        if (animationClip == null || animationClip.length <= 0f)
            return;

        playbackTime = Mathf.Repeat(playbackTime + Time.deltaTime * playbackSpeed, animationClip.length);
        SampleCurrentFrame();
    }

    private void SampleCurrentFrame()
    {
        if (animationClip != null)
            animationClip.SampleAnimation(gameObject, playbackTime);
    }
}
