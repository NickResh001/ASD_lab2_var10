// See https://aka.ms/new-console-template for more information. однофазная сортировка с 3 вспомогательныи файлами

using System;
using System.Text;

internal class Program
{
    //static private int inputSeqSize = 9;
    static void Main(string[] args)
    {
        string inputFilePath = "Input.dat";
        string outputFilePath = "Output.dat";
        FillFile(inputFilePath, 11);
        ShowFile(inputFilePath);
        SortFile2(inputFilePath, outputFilePath);
        ShowFile(outputFilePath);
    }

    static void FillFile(string filepath, int inputSeqSize)
    {
        Random rnd = new Random();
        BinaryWriter inputWriter = new BinaryWriter(File.OpenWrite(filepath));
        for (int i = 0; i < inputSeqSize; i++)
        {
            int value = rnd.Next(0, 100);
            inputWriter.Write(value);
        }
        inputWriter.Close();
    }
    static void ShowFile(string filepath)
    {
        int value;
        Console.WriteLine("===================================");
        BinaryReader reader = new BinaryReader(File.OpenRead(filepath));
        while(reader.Read(new byte[1], 0, 1) != 0)
        {
            reader.BaseStream.Position -= 1;
            value = reader.ReadInt32();
            Console.Write(value + ",\t");
        }
        Console.WriteLine("\n===================================");
        reader.Close();
    }

    static void SortFile(string inputArrayPath)
    {
        const int m = 2; //Сколько будет вспомогательных файлов
        int k = 0; //Номер итерации
        int groupSize = 1;
        int inputFileSize = 0; //Сколько в исходном файле содержится переменных
        string[] filepaths = new string[m * 2]; //Хранение путей к дополнительным файлам
        BinaryReader[] readers = new BinaryReader[m];
        BinaryWriter[] writers = new BinaryWriter[m];

        for (int i = 0; i < m * 2; i++)
        {
            filepaths[i] = $"file{i}.dat";
        }
        BinaryReader inputFileReader = new BinaryReader(File.OpenRead(inputArrayPath));
        for (int i = 0; i < m; i++)
        {
            writers[i] = new BinaryWriter(File.OpenWrite(filepaths[i + k % 2 * m]));
        }

        while (inputFileReader.Read(new byte[1], 0, 1) != 0)
        {
            inputFileReader.BaseStream.Position -= 1;
            writers[inputFileSize % m].Write(inputFileReader.ReadInt32()); //Чтение из изначального файла и разбиение по дополнительным файлам
            inputFileSize++;
        }

        for (int i = 0; i < m; i++)
        {
            writers[i].Close();
        }

        while (groupSize < inputFileSize)
        {
            groupSize = (int)Math.Pow(m, k);
            k++;

            int fileIdx = 0; //В какой дополнительный файл будем записывать
            int noteCount = 0; //Сколько записей уже в дополнительном файле
            int noteValue = 0; //Значение последней записи
            int[] readedIdxs = new int[m]; //Сколько записей считано в фрагменте из определенного файла

            for (int i = 0; i < m; i++)
            {
                readedIdxs[i] = 1;
            }
            int[] readedValues = new int[m]; //Значение считанных записей

            for (int i = 0; i < m; i++)
            {
                File.Delete(filepaths[i + k % 2 * m]);
                writers[i] = new BinaryWriter(File.OpenWrite(filepaths[i + k % 2 * m]));
            }
            for (int i = 0; i < m; i++)
            {
                readers[i] = new BinaryReader(File.OpenRead(filepaths[i + (k - 1) % 2 * m]));
            }
            for (int i = 0; i < m; i++)
            {
                if (readers[i].Read(new byte[1], 0, 1) != 0)
                {
                    readers[i].BaseStream.Position -= 1;
                    readedValues[i] = readers[i].ReadInt32(); //Читаем первые записи в дополнительных файлах
                }
                else
                {
                    readedValues[i] = int.MaxValue;
                    readedIdxs[i] = int.MaxValue;
                }
            }

            bool flag1 = false; //Триггер выхода из цикла
            while (!flag1)
            {
                flag1 = true;
                int excludedFragmentIndex = 0; //Индекс фрагмента из которого будет взята следующая запись
                noteCount++;
                noteValue = readedValues[0]; //Устанавливаем значение добавляемого элемента
                for (int i = 1; i < m; i++)
                {
                    if (readedValues[i] < noteValue)
                    //Находим минимальный элемент из m фрагментов
                    {
                        noteValue = readedValues[i];
                        excludedFragmentIndex = i;
                    }
                }
                if (readedIdxs[excludedFragmentIndex] == Math.Pow(m, k - 1) || noteValue == int.MaxValue) //Проверяем, не записана-ли ещё полная последовательность
                {
                    readedValues[excludedFragmentIndex] = int.MaxValue; //условие завершения итерации записи фрагмента
                }
                else
                {
                    if (readers[excludedFragmentIndex].Read(new byte[1], 0, 1) != 0)
                    {

                        readers[excludedFragmentIndex].BaseStream.Position -= 1;
                        readedValues[excludedFragmentIndex] = readers[excludedFragmentIndex].ReadInt32(); //Считываем новое значение на место вписанной
                        readedIdxs[excludedFragmentIndex]++;
                    }
                    else
                    {
                        readedValues[excludedFragmentIndex] = int.MaxValue; //условие завершения итерации записи фаргмента
                        readedIdxs[excludedFragmentIndex] = int.MaxValue; //условие завершения итерации записи файла
                    }
                }
                if (noteValue != int.MaxValue) //если значение корректно, записываем его в дополнительный файл
                {
                    writers[fileIdx % m].Write(noteValue);
                    Console.WriteLine(noteValue);
                    ////////////////////
                }
                if (noteCount >= Math.Pow(m, k)) //Проверяем, не записан - ли уже полный фрагмент
                {
                    Console.WriteLine($"↑ Итерация: {k}. Запись в вспомогательный файл { fileIdx % m + k % 2 * m}."); ///////////////////////
                    Console.WriteLine("\n"); ////////////////////
                    noteCount = 0;
                    for (int i = 0; i < m; i++)
                    {
                        if (readers[i].Read(new byte[1], 0, 1) != 0) //Считываем из дополнительных файлов новые фрагменты
                        {
                            readers[i].BaseStream.Position -= 1;
                            readedValues[i] = readers[i].ReadInt32();
                            readedIdxs[i] = 1;
                        }
                        else
                        {
                            readedValues[i] = int.MaxValue; //условие завершения итерации записи фаргмента
                            readedIdxs[i] = int.MaxValue; //условие завершения итерации записи файла
                        }
                    }
                    fileIdx++;
                }
                for (int i = 0; i < m; i++)
                {
                    if (readedIdxs[i] != int.MaxValue) //Проверяем, есть - ли ещё значений в дополнительных файлах
                         flag1 = false;
                }
                if (flag1 && noteCount != 0)
                ///////////////////////
                {
                    
                    Console.WriteLine($"↑ Итерация: {k}. Запись в вспомогательный файл { fileIdx % m + k % 2 * m}."); ///////////////////////
                    Console.WriteLine("\n"); ////////////////////
                }
            }
            for (int i = 0; i < m; i++)
            {
                writers[i].Close();
            }
            for (int i = 0; i < m; i++)
            {
                readers[i].Close();
            }
        }
        for (int i = 0; i < m * 2; i++) //Удаляем все дополнительные файлы кроме того, в котором содержится результат сортировки
        {
            if (i == k % 2 * m)
                File.Copy($"file{i}.dat", "ResulArray.dat", true);
            File.Delete($"file{i}.dat");
        }
    }
    static void SortFile2(string inputFilePath, string outputFilePath)
    {
        int m = 2;  // кол-во доп файлов
        int groupSize = 1;
        int inputFileSize = 0;
        int iter = 0;

        string[] filepaths = new string[m * 2];
        for (int i = 0; i < m*2; i++)
        {
            filepaths[i] = $"file{i}.dat";
        }

        BinaryReader inputFileReader = new BinaryReader(File.OpenRead(inputFilePath));
        BinaryReader[] readers = new BinaryReader[m];
        BinaryWriter[] writers = new BinaryWriter[m];
        
        for (int i = 0; i < m; i++)
        {
            writers[i] = new BinaryWriter(File.OpenWrite(filepaths[i + iter % 2 * m]));
        }

        // разделение инпут файла по вспомогательным
        while(inputFileReader.Read(new byte[1], 0, 1) != 0)
        {
            inputFileReader.BaseStream.Position -= 1;
            int buf = inputFileReader.ReadInt32();
            writers[inputFileSize % m].Write(buf);
            Console.WriteLine($"Файл {inputFileSize % m}: Запись: {buf}");
            inputFileSize++;
        }

        for(int i = 0; i < m; i++)
        {
            writers[i].Close();
        }

        int lastFileIdx = 0;
        // итерация цикла - одно слияние-разделение
        while(groupSize < inputFileSize)
        {
            iter++;
            
            for (int i = 0; i < m; i++)
            {
                writers[i] = new BinaryWriter(File.OpenWrite(filepaths[i + iter % 2 * m]));
                readers[i] = new BinaryReader(File.OpenRead(filepaths[i + (iter + 1) % 2 * m]));
            }
            
            int valuesCount = 0;

            //итерация цикла - проходка по всем файлам и запись в каждый по новой группе
            while(valuesCount < inputFileSize)
            {
                //итерация цикла - запись в каждый файл по группе
                for (int i = 0; i < m; i++)
                {
                    bool[] movedFilePointer = new bool[m];
                    int[] readedValues = new int[m];
                    for (int j = 0; j < m; j++)
                    {
                        movedFilePointer[j] = true;
                    }

                    Console.WriteLine($"Итерация {iter}:");
                    //итерация цикла - запись группы в файл
                    for (int j = 0; j < groupSize * m; j++)
                    {
                        //итерация цикла - запись одного числа
                        for (int k = 0; k < m; k++)
                        {
                            if (movedFilePointer[k])
                            {
                                if (readers[k].Read(new byte[1], 0, 1) != 0)
                                {
                                    readers[k].BaseStream.Position -= 1;
                                    readedValues[k] = readers[k].ReadInt32();
                                }
                                else
                                {
                                    readers[k].BaseStream.Position -= 1;
                                    readedValues[k] = Int32.MaxValue;
                                }
                                movedFilePointer[k] = false;
                            }
                        }
                        // заполнили readedValues значениями для сравнения ()

                        int minIdx = MinArr(readedValues);
                        int min = readedValues[minIdx];
                        // получили минимальный элемент

                        // запишем его в файл
                        writers[i].Write(min);
                        valuesCount++;
                        lastFileIdx = i + iter % 2 * m;
                        movedFilePointer[minIdx] = true;
                        readedValues[minIdx] = Int32.MaxValue;

                        Console.WriteLine(min);
                    }
                    Console.WriteLine("=====================");

                }

            }

            for (int i = 0; i < m; i++)
            {
                writers[i].Close();
                readers[i].Close();
            }

            groupSize *= m;
        }

        for(int i = 0; i < m*2; i++)
        {
            if (i == lastFileIdx)
            {
                File.Copy(filepaths[i], outputFilePath, true);
            }
            File.Delete(filepaths[i]);
        }
    }

    static int MinArr(int[] array)
    {
        int min = Int32.MaxValue;
        int minIdx = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] < min)
            {
                min = array[i];
                minIdx = i;
            }
        }
        return minIdx;
    }
    static int MinArr(List<int> list)
    {
        int min = Int32.MaxValue;
        foreach (int i in list)
        {
            if (i < min)
            {
                min = i;
            }
        }
        return min;
    }
}





