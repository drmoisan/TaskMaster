//using System.Collections.Generic;
//using System.Collections;
//using Microsoft.VisualBasic;
//using System;

//namespace UtilitiesCS
//{

//    public class StackObjectCSconv
//    {
//        private ICollection _colObj = new Collection();

//        public void Push(object obj)
//        {
//            _colObj.Add(obj);
//        }

//        public object Pop(int idx = 0)
//        {
//            object PopRet = default;
//            object objTmp;
//            if (idx == 0)
//                idx = _colObj.Count;
//            if (idx > 0)
//            {
//                objTmp = _colObj[idx];
//                _colObj.Remove(idx);
//                PopRet = objTmp;
//            }
//            else
//            {
//                PopRet = null;
//            }

//            return PopRet;
//        }

//        public int Count()
//        {
//            int CountRet = default;
//            CountRet = _colObj.Count;
//            return CountRet;
//        }

//        public Collection ToCollection()
//        {
//            Collection ToCollectionRet = default;
//            ToCollectionRet = _colObj;
//            return ToCollectionRet;
//        }

//        public List<object> ToList()
//        {
//            var listObj = new List<object>();
//            foreach (var objItem in _colObj)
//                listObj.Add(objItem);
//            return listObj;
//        }
//    }
//}