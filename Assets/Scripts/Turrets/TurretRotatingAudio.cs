using UnityEngine;

public class TurretRotatingAudio : MonoBehaviour
{
    public Transform turretTransform; // The part of the turret that rotates
    public AudioSource startMovingSound;
    public AudioSource movingLoopSound;
    public AudioSource stopMovingSound;

    private Quaternion previousRotation;
    private bool isMoving = false;
    private float rotationThreshold = 0.01f; // Threshold to detect rotation changes

    void Start()
    {
        previousRotation = turretTransform.rotation;
    }

    void Update()
    {
        // Compare current and previous rotations
        Quaternion currentRotation = turretTransform.rotation;
        float rotationDelta = Quaternion.Angle(previousRotation, currentRotation);

        if (rotationDelta > rotationThreshold)
        {
            if (!isMoving)
            {
                // Started moving
                isMoving = true;
                PlayStartMovingSound();
                PlayMovingLoopSound();
            }
        }
        else
        {
            if (isMoving)
            {
                // Stopped moving
                isMoving = false;
                PlayStopMovingSound();
                StopMovingLoopSound();
            }
        }

        // Update the previous rotation for the next frame
        previousRotation = currentRotation;
    }

    void PlayStartMovingSound()
    {
        if (startMovingSound != null && !startMovingSound.isPlaying)
        {
            startMovingSound.Play();
        }
    }

    void PlayMovingLoopSound()
    {
        if (movingLoopSound != null && !movingLoopSound.isPlaying)
        {
            movingLoopSound.loop = true;
            movingLoopSound.Play();
        }
    }

    void StopMovingLoopSound()
    {
        if (movingLoopSound != null && movingLoopSound.isPlaying)
        {
            movingLoopSound.Stop();
        }
    }

    void PlayStopMovingSound()
    {
        if (stopMovingSound != null && !stopMovingSound.isPlaying)
        {
            stopMovingSound.Play();
        }
    }
}
