using System;
using System.Collections;

namespace WebApi
{
    public class CamerasInfoItem<T>
    {
        public int CameraNumber { get; set; }
        public T Item { get; set; }
    }

    public class CamerasInfoEnumerator<T> : IEnumerator
    {
        readonly T[] items;
        readonly int firstCamera;
        private int position; 
        public CamerasInfoEnumerator(T[] items, int firstCamera)
        {
            this.items = items;
            this.firstCamera = firstCamera;
            this.position = firstCamera;
        }

        public object Current
        {
            get
            {
                if (position == -1 || position >= items.Length)
                    throw new InvalidOperationException();
                return new CamerasInfoItem<T>
                {
                    CameraNumber = firstCamera + position,
                    Item = items[position]
                };
            }
        }

        public bool MoveNext()
        {
            if (position < items.Length - 1)
            {
                position++;
                return true;
            } 
            return false;
        }

        public void Reset()
        {
            position = -1;
        }
    }

    public class CamerasInfoArray<T>
    {
        public const int FirstCamera = 1;
        public const int LastCamera = 9;
        public const int CameraCount = (LastCamera - FirstCamera) + 1;

        private readonly T[] store = new T[CameraCount];

        public CamerasInfoArray(T[] defaultValues)
        {
            SetInitValues(defaultValues);
        }

        public CamerasInfoArray(T defaultValue) 
        {
            SetInitValues(defaultValue);
        }

        public T this[int camera]
        {
            get
            {
                CheckCameraThrowIfWrong(camera);
                return store[CameraToStoreIndex(camera)];
            }
            set
            {
                if (IsCorrectCameraNumber(camera))
                {
                    store[CameraToStoreIndex(camera)] = value;
                }
            }
        }

       
        private void SetInitValues(T[] defaultValues)
        {
            if (defaultValues.Length != CameraCount)
                throw new ArgumentException($"defaultValues.Length[{defaultValues.Length }] must be == CameraCount[{CameraCount}]");
            for (int i = 0; i < defaultValues.Length; i++)
            {
                store[i] = defaultValues[i];
            }
        }

        private void SetInitValues(T defaultValue)
        {
            for (int i = 0; i < CameraCount; i++)
            {
                store[i] = defaultValue;
            }
        }

        private bool IsCorrectCameraNumber(int camera) => camera >= FirstCamera && camera <= LastCamera;

        private bool IsWrongCameraNumber(int camera) => !IsCorrectCameraNumber(camera);

        private int CameraToStoreIndex(int camera) => camera - FirstCamera;

        private void CheckCameraThrowIfWrong(int camera)
        {
            if (IsWrongCameraNumber(camera))
                throw new ArgumentException($"The camera number has value [{camera}], but it must be in the interval [{FirstCamera}-{LastCamera}]");
        }

    }
}