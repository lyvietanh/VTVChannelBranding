using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VTVPlaylistEditor.Settings
{
    [XmlType(TypeName = "CharacterFilter")]
    public class CharacterFilterSetting
    {
        private bool _enableAsciiCharacter = true;
        private string _userValidCharacter = "áàạãảăắằặẵẳâấầậẫẩéèẹẽẻêếềệễểíìịĩỉúùụũủưứừựữửóòọõỏơớờợỡởôốồộỗổýỳỵỹỷđ";
        private string _convertInvalidCharacterTo = "";

        [XmlAttribute]
        public bool EnableAsciiCharacter
        {
            get
            {
                return _enableAsciiCharacter;
            }

            set
            {
                this._enableAsciiCharacter = value;
            }
        }

        [XmlAttribute]
        public string UserValidCharacter
        {
            get
            {
                return _userValidCharacter;
            }

            set
            {
                this._userValidCharacter = value;
            }
        }

        [XmlAttribute]
        public string ConvertInvalidCharacterTo
        {
            get
            {
                return _convertInvalidCharacterTo;
            }

            set
            {
                this._convertInvalidCharacterTo = value;
            }
        }
    }
}
