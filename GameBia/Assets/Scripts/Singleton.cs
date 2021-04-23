using UnityEngine;

namespace ThreeDPool
{
    //Singleton Pattern là một mẫu thiết kế được sử dụng để bảo đảm rằng mỗi một lớp (class) chỉ có được một thể hiện (instance) duy nhất và mọi tương tác đều thông qua thể hiện này.
    //Singleton Pattern cung cấp một phương thức khởi tạo private, duy trì một thuộc tính tĩnh để tham chiếu đến một thể hiện của lớp Singleton này. Nó cung cấp thêm một phương thức tĩnh trả về thuộc tính tĩnh này
   
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static GameObject _instanceGO;
        private static T _instance;
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    string typeName = typeof(T).Name;

                    //Tìm tên của đối tượng
                    _instanceGO = GameObject.Find(typeName);
                    _instance = _instanceGO.GetComponent<T>();

                    //Đảm bảo rằng chỉ có một đối tượng thuộc loại này ngay tại thời điểm đó
                    if (_instanceGO == null && _instance == null)
                    {
                        //Tạo 1 gameobject empty
                        _instanceGO = new GameObject();

                        //Theo dõi đối tượng theo tên loại của nó
                        _instanceGO.name = typeName;

                        //Tạo đối tượng Singleton
                        _instance = _instanceGO.AddComponent<T>();
                    }


                    //Đảm bảo rằng chỉ có một đối tượng thuộc loại này
                    //Thời gian tồn tại của đối tượng này là thời gian tồn tại của ứng dụng
                    GameObject.DontDestroyOnLoad(_instanceGO);
                }
                //Trả về đối tượng Singleton
                return _instance;
            }
        }

        protected virtual void Init()
        { }

        protected virtual void Awake()
        { }

        protected virtual void Start()
        { }

        protected virtual void Update()
        { }

        protected virtual void OnDestroy()
        {
            _instanceGO = null;
            _instance = null;
        }

    }
}
