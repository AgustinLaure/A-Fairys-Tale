using System.Collections.Generic;
using UnityEngine;

public class UnityPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    private Stack<GameObject> stack;

    private void Awake()
    {
        stack = new Stack<GameObject>();
    }
    public GameObject GetItem()
    {
        if (stack.Count > 0)
        {
            GameObject item = stack.Pop();
            item.SetActive(true);

            return item;
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    public GameObject GetItem(Vector3 position, Quaternion rotation, Transform parent)
    {
        if (stack.Count > 0)
        {
            GameObject item = stack.Pop();
            item.transform.position = position;
            item.transform.rotation = rotation;
            item.transform.SetParent(parent);
            item.SetActive(true);

            return item;
        }
        else
        {
            GameObject newObject = Instantiate(prefab, position, rotation);
            newObject.transform.SetParent(parent, true);
            newObject.SetActive(true);

            return newObject;
        }
    }

    public void ReturnItem(GameObject item)
    {
        item.SetActive(false);
        stack.Push(item);
    }
}
