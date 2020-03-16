using System;
using System.Collections;
using System.Collections.Generic;

namespace VideoReg.Domain
{
    public class CamerasInfoArray<T> : IEnumerable<T>
    {
        const int FirstCamera = 1;
        const int LastCamera = 9;
        const int CameraCount = (LastCamera - FirstCamera) + 1;

        public int firstCamera => FirstCamera;
        public int lastCamera => LastCamera;
        public int cameraCount => cameraCount;

        private readonly T[] store;

        public CamerasInfoArray()
        {
            store = new T[CameraCount];
        }

        public CamerasInfoArray(T[] defaultValues) : base()
        {
            SetInitValues(defaultValues);
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

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return store.GetEnumerator();
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

        private bool IsCorrectCameraNumber(int camera) => camera >= FirstCamera && camera <= LastCamera;

        private bool IsWrongCameraNumber(int camera) => !IsCorrectCameraNumber(camera);

        private int CameraToStoreIndex(int camera) => camera - 1;

        private void CheckCameraThrowIfWrong(int camera)
        {
            if (IsWrongCameraNumber(camera))
                throw new ArgumentException($"The camera number has value [{camera}], but it must be in the interval [{FirstCamera}-{LastCamera}]");
        }

    }
}