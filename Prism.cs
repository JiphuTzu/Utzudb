using UnityEngine;
//============================================================
//@author	JiphuTzu
//@create	2/9/2017
//@company	STHX
//
//@description:正棱柱、正棱锥、正棱台
//============================================================
namespace Hexice.Components
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    //[RequireComponent(typeof(MeshCollider))]
    public class Prism : MonoBehaviour
    {
        public enum Direction{
            Up,
            Forward,
            Right
        }
        [SerializeField]
        private Direction _direction = Direction.Up;
        public Direction direction{
            get{return _direction;}
            set{
                if(_direction==value) return;
                _direction = value;
                Draw();
            }
        }
        [SerializeField, Tooltip("半径——地面顶点到中心的距离")]
        private float _radius = 0.005F;
        public float radius
        {
            get { return _radius; }
            set
            {
                value = Mathf.Max(0F, value);
                if (_radius == value) return;
                _radius = value;
                Draw();
            }
        }
        [SerializeField, Tooltip("高度")]
        private float _height = 0.01F;
        public float height
        {
            get { return _height; }
            set
            {
                value = Mathf.Max(0F, value);
                if (_height == value) return;
                _height = value;
                Draw();
            }
        }
        [SerializeField, Range(3, 20), Tooltip("面数")]
        private int _faces = 3;
        public int faces
        {
            get { return _faces; }
            set
            {
                value = Mathf.Clamp(value, 3, 20);
                if (_faces == value) return;
                _faces = value;
                Draw();
            }
        }
        [SerializeField, Range(0, 1), Tooltip("斜率——0为棱柱,1为棱锥")]
        private float _slope = 1F;
        public float slope
        {
            get { return _slope; }
            set
            {
                value = Mathf.Clamp(value, 0F, 1F);
                if (_slope == value) return;
                _slope = value;
                Draw();
            }
        }
        [SerializeField, HideInInspector]
        private Mesh _mesh;
        public void Draw()
        {
            Init();
            _mesh.Clear();
            if (_radius < 0.001F || _height < 0.001F) return;
            //
            Vector3[] vertices = CreateVertices();
            //
            _mesh.vertices = vertices;
            //_mesh.uv = CreateUVs(vertices);
            _mesh.normals = CreateNormals(vertices);
            _mesh.triangles = CreateTriangles(vertices);

            //GetComponent<MeshCollider>().sharedMesh = _mesh;
        }

        private void Awake()
        {
            Draw();
        }
        private void Init()
        {
            if (_mesh != null) return;
            _mesh = new Mesh();
            _mesh.name = "Prism";
            GetComponent<MeshFilter>().mesh = _mesh;

            Material m = GetComponent<MeshRenderer>().material;
            if (m.name.EndsWith("(Instance)"))
                m.name = m.name.Replace("(Instance)", "");
            m.color = new Color32(0x00,0x80,0x20,0xFF);

            //GetComponent<MeshCollider>().convex = true;
        }
        private int[] CreateTriangles(Vector3[] vertices)
        {
            int i = vertices.Length;
            int[] ts = new int[i];
            while (--i >= 0)
            {
                ts[i] = i;
            }
            return ts;
        }
        private Vector3[] CreateNormals(Vector3[] vertices)
        {
            Vector3[] normals = new Vector3[vertices.Length];
            int index = 0;
            //每个顶点的法线就是所在的面的法线。
            //三角形面的法线为两边的叉乘
            for (int i = 0; i < vertices.Length; i += 3)
            {
                Vector3 normal = Vector3.Cross(vertices[i + 1] - vertices[i], vertices[i + 2] - vertices[i]);
                normals[index++] = normal;
                normals[index++] = normal;
                normals[index++] = normal;
            }
            return normals;
        }
        //TODO:create correct uv
        private Vector2[] CreateUVs(Vector3[] vertices)
        {
            int i = vertices.Length;
            Vector2[] uvs = new Vector2[i];
            while (--i >= 0)
            {
                Vector3 v = vertices[i];
                uvs[i] = new Vector2(v.x, v.z);
            }
            return uvs;
        }
        private Vector3[] CreateVertices()
        {
            if (_slope > 0.999F)
                return CreatePyramidVertices();
            return CreatePrismVertices();
        }
        private Vector3[] CreatePyramidVertices()
        {
            float center = Mathf.Lerp(0.5F,0.67F,_slope);
            float by = (center-1.0F)*_height;
            float ty = center*_height;
            Vector3[] nodes = CreateNodes(by, _radius);
            Vector3[] vertices = new Vector3[(_faces + _faces) * 3];
            int index = 0;
            Vector3 bc = CreateVector3(0, by, 0);
            Vector3 tc = CreateVector3(0, ty, 0);
            //三角形的顶点顺序必须是顺时针，顺时针表示正面，逆时针表示背面，而unity3d在渲染时默认只渲染正面，背面是看不见的。
            for (int i = 0; i < _faces; i++)
            {
                int next = (i + 1) % _faces;
                //bottom
                vertices[index++] = bc;
                vertices[index++] = nodes[next];
                vertices[index++] = nodes[i];
                //side
                vertices[index++] = tc;
                vertices[index++] = nodes[i];
                vertices[index++] = nodes[next];
            }
            return vertices;
        }
        private Vector3[] CreatePrismVertices()
        {
            float center = Mathf.Lerp(0.5F,0.67F,_slope);
            float by = (center-1.0F)*_height;
            float ty = center*_height;
            Vector3[] bottoms = CreateNodes(by, _radius);
            Vector3[] tops = CreateNodes(ty, _radius * (1.0F - _slope));
            Vector3[] vertices = new Vector3[(_faces + _faces + _faces * 2) * 3];
            int index = 0;
            Vector3 bc = CreateVector3(0, by, 0);
            Vector3 tc = CreateVector3(0, ty, 0);
            //三角形的顶点顺序必须是顺时针，顺时针表示正面，逆时针表示背面，而unity3d在渲染时默认只渲染正面，背面是看不见的。
            for (int i = 0; i < _faces; i++)
            {
                int next = (i + 1) % _faces;
                //bottom
                vertices[index++] = bc;
                vertices[index++] = bottoms[next];
                vertices[index++] = bottoms[i];
                //top
                vertices[index++] = tc;
                vertices[index++] = tops[i];
                vertices[index++] = tops[next];
                //side 1
                vertices[index++] = bottoms[i];
                vertices[index++] = bottoms[next];
                vertices[index++] = tops[i];
                //side 2
                vertices[index++] = bottoms[next];
                vertices[index++] = tops[next];
                vertices[index++] = tops[i];
            }
            return vertices;
        }

        private Vector3[] CreateNodes(float y, float radius)
        {
            Vector3[] nodes = new Vector3[_faces];
            float d = -2F * Mathf.PI / (float)_faces;
            for (int i = 0; i < _faces; i++)
            {
                float r = d * i;
                nodes[i] = CreateVector3(Mathf.Cos(r) * radius, y, Mathf.Sin(r) * radius);
            }
            return nodes;
        }
        private Vector3 CreateVector3(float x,float y,float z){
            if(_direction==Direction.Forward){
                return new Vector3(y,z,x);
            }else if(_direction==Direction.Right){
                return new Vector3(z,x,y);
            }else {
                return new Vector3(x,y,z);
            }
        }
        public static Prism Create(string name = "Prism")
        {
            //GameObject go = new GameObject(name);
            //go.name = name;
            return new GameObject(name).AddComponent<Prism>();
        }
    }
}