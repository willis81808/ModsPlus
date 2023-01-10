using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModsPlus
{
    internal class StatusManager : MonoBehaviour
    {
        private Transform _holder;
        private Transform Holder
        {
            get
            {
                if (_holder == null)
                {
                    _holder = new GameObject("Status Group").transform;
                    _holder.SetParent(transform);
                    _holder.localScale = Vector3.one;
                    _holder.localPosition = Vector3.zero;
                }
                return _holder;
            }
        }

        private List<StatusObject> objects = new List<StatusObject>();

        private void Update()
        {
            if (objects.Count == 0) return;

            var offset = Vector3.up * 0.25f;
            foreach (var obj in objects)
            {
                offset += Vector3.up * 0.25f + Vector3.up * obj.verticalPadding;
                obj.transform.localPosition = offset;
                offset += Vector3.up * obj.verticalPadding;
            }
        }

        public void AddStatusObject(GameObject statusObj, float verticalPadding, bool normalizeScale)
        {
            var obj = statusObj.AddComponent<StatusObject>().Initialize(this, verticalPadding);
            obj.transform.SetParent(Holder);

            if (normalizeScale)
            {
                obj.transform.localScale = Vector3.one;
            }

            objects.Add(obj);
        }

        public void RemoveStatusObject(StatusObject objToRemove)
        {
            objects.Remove(objToRemove);
        }
    }

    internal class StatusObject : MonoBehaviour
    {
        private StatusManager parent;

        public float verticalPadding { get; private set; }

        public StatusObject Initialize(StatusManager parent, float verticalPadding)
        {
            this.parent = parent;
            this.verticalPadding = verticalPadding;
            return this;
        }

        private void OnDestroy()
        {
            parent.RemoveStatusObject(this);
        }
    }
}
