using UnityEngine;

public class ObjectProximity : MonoBehaviour
{
    public Transform target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (target)
        {
            // Calculate distance
            var distance = Vector3.Distance(transform.position, target.position);

            Debug.Log($"Distance to target: {distance}");

            if (distance < 5.0f)
                // Do something when close
                UIManager.Instance.ShowDialogue("");
        }
    }
}