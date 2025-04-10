using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Modules.Base
{
    /// <summary>
    /// 提供不同的输入验证方法，名称以 Validate 开头
    /// </summary>
    public abstract class ValidateBase
    {
        /// <summary>
        /// 验证某字符串是否符合验证要求。若符合，返回空字符串；若不符合，返回错误原因。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public abstract string Validate(string str);
    }
}
