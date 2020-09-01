using FileVisualizerIndexer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReadIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;

            var criaPasta = string.Empty;

            do
            {
                Console.WriteLine("Digite o caminho ate o arquivo que deseja realizar a leitura:");
                criaPasta = Console.ReadLine();

            } while (criaPasta != string.Empty && !File.Exists(criaPasta));

            var localArquivo = criaPasta;
            string dataPaths = Path.GetFullPath(localArquivo);

            var quantidadeLinhasExibidas = 11;
            long posicaoInicial = 0;
            long offSetInicial = 0;

            string dataPath = localArquivo;
            string indexPath = FileVisualizerIndexer.Index.GetIndexFileName(dataPath);

            Console.WriteLine("Abrindo Arquivo....");

            using (var index = FileVisualizerIndexer.Index.Create(indexPath, Encoding.Default, 1024 * 1024))
            using (var sReader = new StreamReader(dataPath, Encoding.Default))
            using (var tReader = new TrackingTextReader(sReader))
            {
                string line;
                long position = tReader.Position;
                long lineIndex = 0;
                while ((line = tReader.ReadLine()) != null)
                {
                    index.Add(lineIndex.ToString(), position);
                    position = tReader.Position;
                    lineIndex++;
                }
            }

            Console.Clear();

            Console.WriteLine("Arquivo Aberto");
            Console.WriteLine("- Arrow Down para Avancar as Linhas");
            Console.WriteLine("- Arrow Up para Retroceder as Linhas");
            Console.WriteLine("- Page Up para Avancar 11 Linhas");
            Console.WriteLine("- Page Down para Retroceder 11 Linhas");
            Console.WriteLine("- L para Buscar uma linha em Especifico");
            Console.WriteLine("- C para voltar a Visualizar Atalhos");

            Console.WriteLine("");
            Console.WriteLine("Pressione Enter para Prosseguir");

            Console.ReadKey();

            Console.Clear();


            using (var index = FileVisualizerIndexer.Index.Open(indexPath, Encoding.Default))
            using (var reader = new StreamReader(dataPaths, Encoding.Default))
            {
                var buffer = new List<Buffer>();                
                offSetInicial = index.GetOffsetFrom(posicaoInicial.ToString()).ToList().First();

                ArmazenarCemLinhas(reader, offSetInicial, posicaoInicial, ref buffer);

                LerLinhasArmazenadas(buffer, quantidadeLinhasExibidas);

                do
                {
                    cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.DownArrow)
                    {
                        Console.Clear();
                        posicaoInicial++;
                        bool deveAtualizarBuffer = false;
                        long posicaoAtualizacaoBuffer = 0;

                        if (buffer.Where(b => b.Line >= posicaoInicial).ToList().Count < 50 && posicaoInicial > 89)
                        {
                            offSetInicial = index.GetOffsetFrom((posicaoInicial - 50).ToString()).ToList().First();
                            deveAtualizarBuffer = true;
                            posicaoAtualizacaoBuffer = posicaoInicial - 50;
                        }
                        else
                        {
                            offSetInicial = index.GetOffsetFrom(posicaoInicial.ToString()).ToList().First();
                            deveAtualizarBuffer = !buffer.Where(b => b.Line == posicaoInicial).Any();
                            posicaoAtualizacaoBuffer = posicaoInicial;
                        }

                        var bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();

                        if (deveAtualizarBuffer)
                        {
                            ArmazenarCemLinhas(reader, offSetInicial, posicaoAtualizacaoBuffer, ref buffer);
                            bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();
                        }

                        if (bufferToRead.Count == 0)
                            Console.WriteLine("Fim do Arquivo");
                        else
                            LerLinhasArmazenadas(bufferToRead, quantidadeLinhasExibidas);
                    }
                    else if (cki.Key == ConsoleKey.UpArrow)
                    {
                        Console.Clear();
                        posicaoInicial = (posicaoInicial--) <= 0 ? 0 : posicaoInicial--;
                        bool deveAtualizarBuffer = false;
                        long posicaoAtualizacaoBuffer = 0;


                        if (buffer.Where(b => b.Line <= posicaoInicial).ToList().Count < 50 && posicaoInicial >= 50)
                        {
                            offSetInicial = index.GetOffsetFrom((posicaoInicial - 50).ToString()).ToList().First();
                            deveAtualizarBuffer = true;
                            posicaoAtualizacaoBuffer = posicaoInicial - 50;
                        }
                        else
                        {
                            offSetInicial = index.GetOffsetFrom(posicaoInicial.ToString()).ToList().First();
                            deveAtualizarBuffer = !buffer.Where(b => b.Line == posicaoInicial).Any();
                            posicaoAtualizacaoBuffer = posicaoInicial;
                        }

                        var bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();

                        if (deveAtualizarBuffer)
                        {
                            ArmazenarCemLinhas(reader, offSetInicial, posicaoAtualizacaoBuffer, ref buffer);
                            bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();
                        }

                        LerLinhasArmazenadas(bufferToRead, quantidadeLinhasExibidas);
                    }
                    else if (cki.Key == ConsoleKey.PageDown)
                    {
                        Console.Clear();
                        posicaoInicial = posicaoInicial + 11;
                        bool deveAtualizarBuffer = false;
                        long posicaoAtualizacaoBuffer = 0;


                        if (buffer.Where(b => b.Line >= posicaoInicial).ToList().Count < 50 && posicaoInicial > 89)
                        {
                            offSetInicial = index.GetOffsetFrom((posicaoInicial - 50).ToString()).ToList().First();
                            deveAtualizarBuffer = true;
                            posicaoAtualizacaoBuffer = posicaoInicial - 50;
                        }
                        else
                        {
                            offSetInicial = index.GetOffsetFrom(posicaoInicial.ToString()).ToList().First();
                            deveAtualizarBuffer = !buffer.Where(b => b.Line == posicaoInicial).Any();
                            posicaoAtualizacaoBuffer = posicaoInicial;
                        }

                        var bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();

                        if (deveAtualizarBuffer)
                        {
                            ArmazenarCemLinhas(reader, offSetInicial, posicaoAtualizacaoBuffer, ref buffer);
                            bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();
                        }


                        if (bufferToRead.Count == 0)
                            Console.WriteLine("Fim do Arquivo");
                        else
                            LerLinhasArmazenadas(bufferToRead, quantidadeLinhasExibidas);


                    }
                    else if (cki.Key == ConsoleKey.PageUp)
                    {
                        Console.Clear();
                        posicaoInicial = (posicaoInicial - 11) <= 0 ? 0 : posicaoInicial - 10;
                        bool deveAtualizarBuffer = false;
                        long posicaoAtualizacaoBuffer = 0;


                        if (buffer.Where(b => b.Line <= posicaoInicial).ToList().Count < 50 && posicaoInicial >= 50)
                        {
                            offSetInicial = index.GetOffsetFrom((posicaoInicial - 50).ToString()).ToList().First();
                            deveAtualizarBuffer = true;
                            posicaoAtualizacaoBuffer = posicaoInicial - 50;
                        }
                        else
                        {
                            offSetInicial = index.GetOffsetFrom(posicaoInicial.ToString()).ToList().First();
                            deveAtualizarBuffer = !buffer.Where(b => b.Line == posicaoInicial).Any();
                            posicaoAtualizacaoBuffer = posicaoInicial;
                        }


                        var bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();

                        if (deveAtualizarBuffer)
                        {
                            ArmazenarCemLinhas(reader, offSetInicial, posicaoAtualizacaoBuffer, ref buffer);
                            bufferToRead = buffer.Where(b => b.Line >= posicaoInicial).ToList();
                        }

                        LerLinhasArmazenadas(bufferToRead, quantidadeLinhasExibidas);
                    }
                    else if (cki.Key == ConsoleKey.L)
                    {
                        Console.Clear();
                        Console.WriteLine("Digite a Linha para navegar:");
                        var inputPosicaoUsuario = Console.ReadLine();
                        posicaoInicial = Convert.ToInt64(inputPosicaoUsuario) - 6;
                        var linhaDireta = Convert.ToInt32(posicaoInicial);


                        offSetInicial = index.GetOffsetFrom(posicaoInicial.ToString()).ToList().First();

                        ArmazenarCemLinhas(reader, offSetInicial, posicaoInicial, ref buffer);

                        LerLinhasArmazenadas(buffer, quantidadeLinhasExibidas);
                    }
                } while (cki.Key != ConsoleKey.Escape);
            }

            File.Delete(indexPath);
            File.Delete(indexPath + ".bkt");
        }
        private static void ArmazenarCemLinhas(StreamReader leitor, long offsetInicial, long actualLine, ref List<Buffer> buffer)
        {
            buffer.Clear();
            leitor.BaseStream.Position = offsetInicial;
            leitor.DiscardBufferedData();

            long position = leitor.BaseStream.Position;
            var counter = 0;
            string textoTotal = string.Empty;
            while (!leitor.EndOfStream)
            {
                string linhaAtual = leitor.ReadLine();                
                textoTotal += $"{linhaAtual} {Environment.NewLine}";

                buffer.Add(new Buffer() { Content = linhaAtual, Line = actualLine});

                counter++;
                actualLine++;
                if (counter >= 100)
                    break;
            }
        }

        private static void LerLinhasArmazenadas(List<Buffer> buffer, int quantidadeLinhasExibidas)
        {
            var counter = 0;
            string textoTotal = string.Empty;            

            foreach (var b in buffer)
            {
                textoTotal += $"{b.Content} {Environment.NewLine}";

                counter++;
                if (counter >= quantidadeLinhasExibidas)
                    break;
            }            

            Console.WriteLine(textoTotal);

        }
    }
}
