﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit;
using System.Diagnostics;

namespace CryptoProgramma
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region properties encrypteren

        private static string FileForEncrypt = "";
        // static string PadRSAKeys = "C\\"; // --> Deze waarde is nooit gebruikt
        //********Daniela begin ********************
        static string hoofdPad = "C:\\DocumentenCrypto\\";
        string filename = "";
        string[] opgeslagenBestanden = new string[8];
        //bestandnaam (.txt) 0 publickeySender, 1 privatekeySender, 2 publickeyreceiver, 3 privatekeyreceiver
        // 4 pad privatekeysender, 5 pad publickeySender, 6 pad privatekeyreceiver, 7 pad publickeyreceiver,

        Microsoft.Win32.OpenFileDialog browseVenster = new Microsoft.Win32.OpenFileDialog();
        //********Daniela end ********************

        // edit nasim
        string encryptedFilePath;
        string encryptedFileName;
        static string sKey;
        DES des = new DES();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            
        }

        #region encription window

        #region home-back-browsebuttons encryption windows

        /// <summary>
        /// Button to open the encrypt window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void encryptHomeButton_Click(object sender, RoutedEventArgs e)
        {            //Daniela
            //mainTabs.SelectedItem = mainTabs.FindName("encryptFile");
            encryptFileGrid.Visibility = Visibility.Visible;
            homePageGrid.Visibility = Visibility.Collapsed;
            padEnFileLbl.Content = "";
            senderTxt.Text = "";
            receiverTxt.Text = "";
            //Daniela
        }

        /// <summary>
        /// Buton to return to the home-window from the encryptwindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backButton_EnFileGr_Click(object sender, RoutedEventArgs e)
        {            //Daniela
            homePageGrid.Visibility = Visibility.Visible;
            encryptFileGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Button to browse to de file for encryption
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseEnBtn_Click(object sender, RoutedEventArgs e)
        {            //Daniela
            browseVenster.Filter = "Txt Documents|*.txt";
            if (browseVenster.ShowDialog() == true)
            {
                FileForEncrypt = browseVenster.FileName;
                padEnFileLbl.Content = browseVenster.FileName;
            }
        }

        /// <summary>
        /// Button to return to the home-window after encrypting a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backBtn_EnGr_Click(object sender, RoutedEventArgs e)
        {
            //********Daniela begin ********************

            homePageGrid.Visibility = Visibility.Visible;
            encryptingGrid.Visibility = Visibility.Collapsed;
            CryptoProgramWin.Height = 500;
            CryptoProgramWin.Width = 500;
            //********Daniela end ********************

        }

        #endregion

        #region encryptioncode to encrypt de file, generate RSA key & AES, DES semetric keys
        /// <summary>
        /// Button to encryption the file that's given
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            //public en private keys gemaakt en gesaved
            //Daniela
            opgeslagenBestanden = RSA.keys(hoofdPad, senderTxt.Text, receiverTxt.Text);
            //de publieke en private sleutel worden gegenereerd en opgeslagen


            if (FileForEncrypt != "" && FileForEncrypt != " ")
            {
                //Hier word de symetric AESkey genereert 
                string filetext = File.ReadAllText(FileForEncrypt);
                //lander
                //hash gemaakt van bestand
                statusLbl.Content = "Hashing file (MD5)";
                encrProgressbar.Value = 0;
                string md5sum = hash(FileForEncrypt);
                // Console.WriteLine(md5sum);
                //lander

                //********Daniela begin ********************

                //public en private keys gemaakt en gesaved
                opgeslagenBestanden = RSA.keys(hoofdPad, senderTxt.Text, receiverTxt.Text);

                //hash geencrypteert en opgeslaan
                string encryptHash = RSA.Encrypt(md5sum, opgeslagenBestanden[4]); //encrypteren met private key sender
                Directory.CreateDirectory(hoofdPad);
                string hashfilename = System.IO.Path.GetFileNameWithoutExtension(FileForEncrypt);
                //if (!System.IO.File.Exists(hoofdPad + "Hash" + hashfilename + ".txt"))
                //{
                    File.WriteAllText(hoofdPad + "Hash" + hashfilename + ".txt", encryptHash);
                //}
                nameEnHashLbl.Content = "Hash" + hashfilename + ".txt";
                padEncryptedHash.Content = hoofdPad + "Hash" + hashfilename + ".txt";
                checkBox7.IsChecked = true;


                encryptingGrid.Visibility = Visibility.Visible;
                encryptFileGrid.Visibility = Visibility.Collapsed;
                CryptoProgramWin.Height = 700;
                CryptoProgramWin.Width = 550;
                //********Daniela end ********************

                if (sKeySlider.Value == 2)
                {
                    statusLbl.Content = "Preparing (AES)";
                    encrProgressbar.Value = 10;
                    //****************door Nasim toegevoed*******************
                    string plainFilePath = padEnFileLbl.Content.ToString();
                    encryptedFileName = SplitNameOfFile(plainFilePath, "AES", ".txt");
                    //Krijg anders error als ik Keys map verwijder? Maakt deze gewoon aan indien deze nog niet bestaat
                    System.IO.Directory.CreateDirectory(hoofdPad);
                    encryptedFilePath = hoofdPad + encryptedFileName;

                    byte[] encryptionKey = GenerateRandom(16);
                    byte[] encryptionIV = GenerateRandom(16);
                    byte[] signatureKey = GenerateRandom(64);


                    statusLbl.Content = "Encrypting (AES) ";
                    encrProgressbar.Value = 20;
                    AES.EncryptFile(plainFilePath, encryptedFilePath, encryptionKey, encryptionIV);
                    statusLbl.Content = "Finished (AES)";
                    encrProgressbar.Value = 100;

                    // tonen meer info over encrypteren
                    // System.Windows.MessageBox.Show(string.Format(AES.CreateEncryptionInfoXml(signatureKey, encryptionKey, encryptionIV)), "Info about encryption", MessageBoxButton.OK, MessageBoxImage.Information);
                    //*************************END**************************
                    //********Daniela begin ********************
                    //opslaan en encrypteren symetrisch AES key
                    //werkt nog nie (geeft errors)
                    //keys zijn in byte en om te encrypteren heb ik een string nodig

                    //Nasim - Nu werkt ht wel :p
                    Directory.CreateDirectory(hoofdPad);
                    string encryptAESSkey = RSA.Encrypt(Convert.ToString(encryptionKey), opgeslagenBestanden[7]);
                    filename = System.IO.Path.GetFileNameWithoutExtension(plainFilePath);
                    if (!System.IO.File.Exists(hoofdPad + "SymetricKeyAES" + filename + ".txt"))
                    {
                        File.WriteAllText(hoofdPad + "SymetricKeyAES" + filename + ".txt", encryptAESSkey);
                    }

                    nameEnSymKLbl.Content = "SymetricKeyAES" + filename + ".txt";
                    padEncryptedSkey.Content = hoofdPad + "SymetricKeyAES" + filename + ".txt";
                    checkBox6.IsChecked = true;
                    //********Daniela end ********************


                }
                else if (sKeySlider.Value == 1)
                {
                    //****************door Nasim toegevoed*******************

                    statusLbl.Content = "Preparing (DES)";
                    encrProgressbar.Value = 10;

                    sKey = des.GenerateKey();

                    string source = padEnFileLbl.Content.ToString();
                    encryptedFileName = SplitNameOfFile(source, "DES", ".txt");
                    //Krijg anders error als ik Keys map verwijder? Maakt deze gewoon aan indien deze nog niet bestaat
                    System.IO.Directory.CreateDirectory(hoofdPad);
                    encryptedFilePath = hoofdPad + encryptedFileName;
                    string destination = encryptedFilePath;

                    statusLbl.Content = "Encrypting (DES)";
                    encrProgressbar.Value = 20;

                    des.EncryptFile(source, destination, sKey);

                    statusLbl.Content = "Finished (DES)";
                    encrProgressbar.Value = 100;
                    // System.Windows.MessageBox.Show("Succesfully Encrypted!", "Info about encryption", MessageBoxButton.OK, MessageBoxImage.Information);
                    //*************************END**************************

                    //********Daniela begin ********************
                    //opslaan en encrypteren symetrisch DES key
                    string encryptDESSkey = RSA.Encrypt(sKey, opgeslagenBestanden[7]);
                    Directory.CreateDirectory(hoofdPad);
                    filename = System.IO.Path.GetFileNameWithoutExtension(source);
                    //if (!System.IO.File.Exists(hoofdPad + "SymetricKeyDES" + filename + ".txt"))
                    //{
                        File.WriteAllText(hoofdPad + "SymetricKeyDES" + filename + ".txt", encryptDESSkey);
                    //}
                    nameEnSymKLbl.Content = "SymetricKeyDES" + filename + ".txt";
                    padEncryptedSkey.Content = hoofdPad + "SymetricKeyDES" + filename + ".txt";
                    checkBox6.IsChecked = true;
                    //********Daniela end ********************
                }

                
                //********Daniela begin ********************


                namePrKeySenderLbl.Content = Convert.ToString(opgeslagenBestanden[1]);
                padPrivateSenderLbl.Content = Convert.ToString(opgeslagenBestanden[4]);
                checkBox1.IsChecked = true;
                //toont de naam en pad van de private key zender

                namePuKeySenderLbl.Content = Convert.ToString(opgeslagenBestanden[0]);
                padPublicSenderLbl.Content = Convert.ToString(opgeslagenBestanden[5]);
                checkBox2.IsChecked = true;
                //toont de naam en pad van de publieke key zender

                namePrKeyReceiverLbl.Content = Convert.ToString(opgeslagenBestanden[3]);
                padPrivateReceiverLbl.Content = Convert.ToString(opgeslagenBestanden[6]);
                checkBox3.IsChecked = true;
                //toont de naam en pad van de private key ontvanger

                namePuKeyReceiverLbl.Content = Convert.ToString(opgeslagenBestanden[2]);
                padPublicReceiverLbl.Content = Convert.ToString(opgeslagenBestanden[7]);
                checkBox4.IsChecked = true;
                //toont de naam en pad van de private key ontvanger
                //******Daniela einde*****

                padEncryptedFile.Content = encryptedFilePath;
                nameEnFileLbl.Content = encryptedFileName;
                checkBox5.IsChecked = true;
            }
            else
            {
                System.Windows.MessageBox.Show("Je heb nog geen bestand gekozen");
            }
        }

        #endregion


        #endregion

        #region decription window

        #region home-back-browsebuttons decryption windows
        /// <summary>
        /// Button to go to the decryptionpage from the homepage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void decryptHomeButton_Click(object sender, RoutedEventArgs e)
        {
            //********Daniela begin ********************

            //mainTabs.SelectedItem = mainTabs.FindName("decryptFile");
            decryptFileGrid.Visibility = Visibility.Visible;
            homePageGrid.Visibility = Visibility.Collapsed;
            //********Daniela end ********************

        }

        /// <summary>
        /// Button to go to the homepage from de decyptionpage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backButton_DeFileGr_Click(object sender, RoutedEventArgs e)
        {
            //********Daniela begin ********************

            homePageGrid.Visibility = Visibility.Visible;
            decryptFileGrid.Visibility = Visibility.Collapsed;
            //********Daniela end ********************

        }

        /// <summary>
        /// Button to go to the homepage after decrypting a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backBtn_DeGr_Click(object sender, RoutedEventArgs e)
        {
            //********Daniela begin ********************

            homePageGrid.Visibility = Visibility.Visible;
            decryptingGrid.Visibility = Visibility.Collapsed;
            //********Daniela end ********************
        }

        #region browse buttons
        /// <summary>
        /// Button to go to browse to the file to decrypt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseFileBut_Click(object sender, RoutedEventArgs e)
        {
            browseVenster.Filter = "Txt Documents|*.txt";
            if (browseVenster.ShowDialog() == true)
            {
                decryptFile = browseVenster.FileName;
                fileLbl.Content = browseVenster.FileName;
            }

        }

        /// <summary>
        /// Button to go to browse to the encrypted symetric key file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseSemKeyBut_Click(object sender, RoutedEventArgs e)
        {
            browseVenster.Filter = "Txt Documents|*.txt";
            if (browseVenster.ShowDialog() == true)
            {
                decryptSemKey = browseVenster.FileName;
                symkeyLbl.Content = browseVenster.FileName;
            }

        }

        /// <summary>
        /// Button to go to browse to the encrypted hashfile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseHashBut_Click(object sender, RoutedEventArgs e)
        {
            browseVenster.Filter = "Txt Documents|*.txt";
            if (browseVenster.ShowDialog() == true)
            {
                decryptHash = browseVenster.FileName;
                hashLbl.Content = browseVenster.FileName;
            }

        }

        /// <summary>
        /// Button to go to browse to the public key file of the sender
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browsePublicBut_Click(object sender, RoutedEventArgs e)
        {
            browseVenster.Filter = "Txt Documents|*.txt";
            if (browseVenster.ShowDialog() == true)
            {
                pubSender = browseVenster.FileName;
                publicLbl.Content = browseVenster.FileName;
            }

        }

        /// <summary>
        /// Button to go to browse to the private key file of the receiver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browsePrivateBut_Click(object sender, RoutedEventArgs e)
        {
            browseVenster.Filter = "Txt Documents|*.txt";
            if (browseVenster.ShowDialog() == true)
            {
                privReceiv = browseVenster.FileName;
                privateLbl.Content = browseVenster.FileName;
            }

        }
        #endregion

        #endregion


        #region decryptioncode to decrypt the file. And use of the RSA keys & AES, DES semetric keys

        #region properties decrypteren 
        string decryptFile, decryptSemKey, decryptHash, pubSender, privReceiv;
        //properties decrypteren

        #endregion

        /// <summary>
        /// Button to go decrypt all given files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {

            decryptingGrid.Visibility = Visibility.Visible;
            decryptFileGrid.Visibility = Visibility.Collapsed;
            if (fileLbl.Content.Equals("") || symkeyLbl.Content.Equals("") || hashLbl.Content.Equals("") || publicLbl.Content.Equals("") || privateLbl.Content.Equals(""))
            {
                System.Windows.MessageBox.Show("Je mist een bestand, controleer of je alles heb gekozen");
            }
            else
            {
                //stap 1 : symetric decrypteren met privesleutel
                string inhouddecryptSemKey = File.ReadAllText(decryptSemKey);
                string ontcijferdeSemKey = RSA.Decrypt(inhouddecryptSemKey, privReceiv);
                Directory.CreateDirectory(hoofdPad + "\\DecryptedFiles");
                File.WriteAllText(hoofdPad + "\\DecryptedFiles\\" + "DecryptedSkey" +
                    System.IO.Path.GetFileNameWithoutExtension(decryptFile) + ".txt", ontcijferdeSemKey);

                //stap 2:  bestand decrypteren met semetric key
                //bij het decrypteren komt ook een error idk why

                //Nasim - Nu komt geen error :p 
                string destination = hoofdPad + "DecryptedFiles\\" + "DecryptedTxt" +
                                     System.IO.Path.GetFileNameWithoutExtension(decryptFile) + ".txt";
                des.DecryptFile(decryptFile, destination, ontcijferdeSemKey);
                Process.Start(destination);



                //stap 3 : hash berekenen boodschap

                //Onderstaande code zou moeten werken maar geeft een error 
                //stap 4 : hash decryperen met publiekesleutel
                //string inhouddecryptHash = File.ReadAllText(decryptHash);
                //string ontcijferdeHash = RSA.Decrypt(inhouddecryptHash, pubSender);
                //Directory.CreateDirectory(hoofdPad + "\\DecryptedFiles");
                //File.WriteAllText(hoofdPad + "\\DecryptedFiles\\" + "DecryptedHash" +
                //    System.IO.Path.GetFileNameWithoutExtension(decryptHash) + ".txt", ontcijferdeHash);

                //stap 5 : zelfberekende hash en ontcijferde hash vergelijken 

            }
        }

        #endregion


        #endregion

        #region helper functions

        /// <summary>
        /// Generate random byte array
        /// </summary>
        /// <param name="length">array length</param>
        /// <returns>Random byte array</returns>
        private static byte[] GenerateRandom(int length)
        {
            byte[] bytes = new byte[length];
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
            {
                random.GetBytes(bytes);
            }
            return bytes;
        }

        /// <summary>
        /// Function to split the name of a file | 
        /// Editor: Nasim
        /// </summary>
        /// <param name="plainFilePath"></param>
        /// <param name="algirithme"></param>
        /// <param name="newSuffix"></param>
        /// <returns></returns>
        private static string SplitNameOfFile(string plainFilePath, string algirithme, string newSuffix)
        {
            return System.IO.Path.GetFileNameWithoutExtension(plainFilePath) + algirithme + newSuffix;
        }

        /**
        * <summary>
        * Calculates the specified hash of a message
        * </summary>
        * <param name="message">The message</param>
        * <param name="type">The type of hash (for example: "MD5" or "SHA1")</param>
        * <returns>A hash of message</returns>
        */
        private String hash(String message, String type)
        {
            byte[] enc = new UTF8Encoding().GetBytes(message);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName(type)).ComputeHash(enc);
            return BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
        }

        /**
         * <summary>
         * Calculates the MD5 hash of a specified file
         * </summary>
         * <param name="path">The path to the file</param>
         * <returns>MD5 hash of specified file</returns>
         */
        private String hash(String path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty).ToLower();
                }
            }
        }
        #endregion

        /// <summary>
        /// Button to go to the homepage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void home_Menu_Selected(object sender, RoutedEventArgs e)
        {
            homePageGrid.Visibility = Visibility.Visible;
            encryptFileGrid.Visibility = Visibility.Collapsed;
            encryptingGrid.Visibility = Visibility.Collapsed;
            decryptFileGrid.Visibility = Visibility.Collapsed;
            decryptingGrid.Visibility = Visibility.Collapsed;
            CryptoProgramWin.Height = 500;
            CryptoProgramWin.Width = 500;
        }
        
        #region stenografie window, props and actions

        /// <summary>
        /// Button to get out of the stenografie-window and go to the homepage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void backSteg_Btn_Click(object sender, RoutedEventArgs e)
        {
            CryptoProgramWin.Height = 500;
            CryptoProgramWin.Width = 500;
            steganografieGrid.Visibility = Visibility.Collapsed;
            homePageGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Button to open the stenografie-options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stega_Menu_Selected(object sender, RoutedEventArgs e)
        {
            CryptoProgramWin.Height = 500;
            CryptoProgramWin.Width = 500;
            steganografieGrid.Visibility = Visibility.Visible;
            homePageGrid.Visibility = Visibility.Collapsed;
            encryptFileGrid.Visibility = Visibility.Collapsed;
            encryptingGrid.Visibility = Visibility.Collapsed;
            decryptFileGrid.Visibility = Visibility.Collapsed;
            decryptingGrid.Visibility = Visibility.Collapsed;
            settingsGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Selecting the image that will be used for the stenography
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseImageButton_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog open_dialog = new OpenFileDialog();
            //open_dialog.Filter = "Image Files (*.jpeg; *.png; *.bmp)|*.jpg; *.png; *.bmp";

            //if (open_dialog.ShowDialog() == DialogResult.OK)
            //{
            //    selectedImage.Image = Image.FromFile(open_dialog.FileName);
            //    maakLeeg();
            //}


            browseVenster.Filter = "Image Files (*.jpeg; *.png; *.bmp)| *.jpg; *.png; *.bmp";
            if (browseVenster.ShowDialog() == true)
            {
                selectedImage.Source = Convert.ToString(browseVenster.FileName);
                labelSelectedImage.Content = browseVenster.FileName;
            }
        }

        /// <summary>
        /// Selecting the file that will be used for the stenography
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browsFileButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        #region settings window

        /// <summary>
        /// Button to open the settings-window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settings_Menu_Selected(object sender, RoutedEventArgs e)
        {
            //********Daniela begin ********************

            CryptoProgramWin.Height = 500;
            CryptoProgramWin.Width = 500;
            settingsGrid.Visibility = Visibility.Visible;
            homePageGrid.Visibility = Visibility.Collapsed;
            encryptFileGrid.Visibility = Visibility.Collapsed;
            encryptingGrid.Visibility = Visibility.Collapsed;
            decryptFileGrid.Visibility = Visibility.Collapsed;
            decryptingGrid.Visibility = Visibility.Collapsed;
            steganografieGrid.Visibility = Visibility.Collapsed;
            rsaKeys_lbl.Content = hoofdPad;
            //********Daniela end ********************

        }

        /// <summary>
        /// Button to edit the path to save the rsakeys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rsaKeys_ChangeBtn_Click(object sender, RoutedEventArgs e)
        { //kevin
            FolderBrowserDialog browseFolder = new FolderBrowserDialog();
            browseFolder.ShowDialog();
            hoofdPad = browseFolder.SelectedPath + "\\";
        }
        
        /// <summary>
        /// Button to edit the backgroundcolor of the entire program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedEventArgs e)
        {       //kevin
            Brush brush = new SolidColorBrush(ClrPcker_Background.SelectedColor.Value);
            SideMenu.Background = brush;
        }

        #endregion

        #region help window

        /// <summary>
        /// Button to open the help-menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void help_Menu_Selected(object sender, RoutedEventArgs e)
        {
            //********Daniela begin ********************
            CryptoProgramWin.Height = 500;
            CryptoProgramWin.Width = 500;
            helpGrid.Visibility = Visibility.Visible;
            homePageGrid.Visibility = Visibility.Collapsed;
            encryptFileGrid.Visibility = Visibility.Collapsed;
            encryptingGrid.Visibility = Visibility.Collapsed;
            decryptFileGrid.Visibility = Visibility.Collapsed;
            decryptingGrid.Visibility = Visibility.Collapsed;
            steganografieGrid.Visibility = Visibility.Collapsed;
            settingsGrid.Visibility = Visibility.Collapsed;
            //********Daniela end ********************

        }
        #endregion
       
        #region exit program
        /// <summary>
        /// Button to exit the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_Menu_Selected(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        //Override the onClose method in the Application Main window
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Wilt u de applicatie afsluiten?", "EXIT",
                                                  MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
        #endregion
    }
}
/* Bronnen:  https://github.com/alicanerdogan/HamburgerMenu */
