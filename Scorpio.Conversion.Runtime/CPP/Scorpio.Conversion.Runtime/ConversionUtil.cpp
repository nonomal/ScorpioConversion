#include "ConversionUtil.h"
namespace Scorpio {
	namespace Conversion {
		namespace Runtime {
			void ConversionUtil::ReadHead(IReader* reader) {
                {
                    int number = reader->ReadInt32();    //��ṹ�ֶ�����
                    for (int i = 0; i < number; ++i) {
                        if (reader->ReadInt8() == 0) {   //��������
                            reader->ReadInt8();          //������������
                        }
                        else {                        //�Զ�����
                            reader->ReadString();        //�Զ���������
                        }
                        reader->ReadBool();              //�Ƿ�������
                        reader->ReadString();            //�ֶ�����
                    }
                }
                {
                    int customNumber = reader->ReadInt32();  //�Զ���������
                    for (int i = 0; i < customNumber; ++i) {
                        reader->ReadString();                //��ȡ�Զ���������
                        if (reader->ReadInt8() == 1) {
                            int number = reader->ReadInt32();
                            for (int j = 0; j < number; ++j) {
                                reader->ReadString();
                                reader->ReadInt32();
                            }
                        }
                        else {
                            int number = reader->ReadInt32();    //�ֶ�����
                            for (int j = 0; j < number; ++j) {
                                if (reader->ReadInt8() == 0) {   //��������
                                    reader->ReadInt8();          //������������
                                }
                                else {                        //�Զ�����
                                    reader->ReadString();        //�Զ���������
                                }
                                reader->ReadBool();              //�Ƿ�������
                                reader->ReadString();
                            }
                        }
                    }
                }
			}
		}
	}
}