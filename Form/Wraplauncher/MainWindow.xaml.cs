using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;

namespace WrapLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // パスファイル（BOMなしUTF8）
        private const string ConfigFileName = "WrapLauncher.path";
        // 見出しテキストのカラム位置
        private const int IdxTitle = 0;
        // 色記号のカラム位置
        private const int IdxColor = 0;
        // ボタンテキストのカラム位置
        private const int IdxBtnText = 1;
        // 起動フォルダパスのカラム位置
        private const int IdxLaunchPath = 2;
        // 色テーブル
        private static readonly Dictionary<string, SolidColorBrush>ColorTable = new Dictionary<string, SolidColorBrush>
        {
            { "bla", new SolidColorBrush(Color.FromArgb(0xff, 0x00, 0x00, 0x00)) },
            { "gra", new SolidColorBrush(Color.FromArgb(0xff, 0x80, 0x80, 0x80)) },
            { "yel", new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xd7, 0x00)) },
            { "gre", new SolidColorBrush(Color.FromArgb(0xff, 0x00, 0x80, 0x00)) },
            { "blu", new SolidColorBrush(Color.FromArgb(0xff, 0x00, 0x00, 0xff)) },
            { "sky", new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0xce, 0xeb)) },
            { "red", new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x00, 0x00)) },
            { "ora", new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x8c, 0x00)) },
            { "pur", new SolidColorBrush(Color.FromArgb(0xff, 0x80, 0x00, 0x80)) },
            { "whi", new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff)) },
        };
        // ヘルプテキスト
        private const string HelpText = @"設定ファイルはEXEと同じ場所に「WrapLauncher.path」のファイル名で配置する。
文字コードはBOM無しのUTF8。

データ構造
------------------------------
//見出し
色記号  ボタンテキスト   起動プログラム/フォルダのフルパス
色記号  ボタンテキスト   起動プログラム/フォルダのフルパス
：
//見出し
色記号  ボタンテキスト   起動プログラム/フォルダのフルパス
：
------------------------------
見出しは先頭「//」で始める。
色記号、ボタンテキスト、起動するパスはTABで区切る。
使用できる色記号は
bla：黒、gra：灰色、yel：黄色、gre：緑、blu：青
sky：水色、red：赤、ora：オレンジ、pur：紫、whi：白
";


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // コンテキストメニュー設定
            this.ContextMenu = (ContextMenu)(this.Resources["CtxMenu"]);
        }

        /// <summary>
        /// 起動時
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 設定読み込み、画面に反映
            LoadConfig();
        }

        /// <summary>
        /// コンテキストメニュー「設定再読み込み」
        /// </summary>
        private void MenuReload_Click(object sender, RoutedEventArgs e)
        {
            // 画面クリア
            mainContainer.Children.Clear();

            // 設定読み込み、画面に反映
            LoadConfig();
        }

        /// <summary>
        /// コンテキストメニュー「ヘルプ」
        /// </summary>
        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(HelpText);
        }

        /// <summary>
        /// 設定読み込み、画面に反映
        /// </summary>
        private void LoadConfig()
        {
            // 設定ファイルのフルパス取得
            string cfgFilePath = GetConfigFilePath();

            // 設定ファイルが見つからなければ終了
            if (string.IsNullOrEmpty(cfgFilePath))
            {
                return;
            }

            // 設定ファイル読み込み
            using (var reader = new System.IO.StreamReader(cfgFilePath))
            {
                WrapPanel btnContainer = null;

                while (!reader.EndOfStream)
                {
                    // TABで分解
                    var item = reader.ReadLine().Split('\t');
                    if (item.Length < 1)
                    {
                        // データなしの行
                        continue;
                    }

                    /*
                    mainContainer           ..... StackPanel
                        grpContainer        ..... StackPanel
                            見出し          ..... TextBlock
                            btnContainer    ..... WrapPanel
                                ボタン
                                ボタン
                                ：
                        grpContainer        ..... StackPanel
                            見出し          ..... TextBlock
                            btnContainer    ..... WrapPanel
                                ボタン
                                ボタン
                                ：
                    */

                    // グループ作成メソッド
                    void MakeGroup(ref WrapPanel btnContainer, string grpTitle = "")
                    {
                            // グループコンテナ作成
                            var grpContainer = new StackPanel();
                            // グループコンテナをメインコンテナに追加
                            mainContainer.Children.Add(grpContainer);

                            if (string.IsNullOrEmpty(grpTitle) == false)
                            {
                                // 見出し作成
                                var txt = new TextBlock();
                                txt.Style = (Style)(this.Resources["TitleLabel"]);
                                txt.Text = grpTitle;
                                // 見出しをグループコンテナに追加
                                grpContainer.Children.Add(txt);
                            }

                            // ボタンコンテナを作成
                            btnContainer = new WrapPanel();
                            // ボタンコンテナをグループコンテナに追加
                            grpContainer.Children.Add(btnContainer);
                    }

                    if (item[IdxTitle].StartsWith("//"))
                    {
                        // 見出し

                        // グループ作成
                        MakeGroup(ref btnContainer, item[IdxTitle].Substring(2));
                    }
                    else if (item.Length == 3)
                    {
                        // ボタン

                        // まだボタンコンテナが作成されていない？
                        if (btnContainer == null)
                        {
                            // グループ作成（見出し無し）
                            MakeGroup(ref btnContainer);
                        }

                        // ボタン作成
                        var btn = new Button();
                        SetButtonProperty(ref btn, item[IdxColor], item[IdxBtnText], item[IdxLaunchPath]);
                        // ボタンコンテナにボタンを追加
                        btnContainer.Children.Add(btn);
                    }
                }
            }
        }

        /// <summary>
        /// ボタン属性設定
        /// </summary>
        /// <param name="btn">ボタンオブジェクト</param>
        /// <param name="colorKey">色</param>
        /// <param name="text">ボタンテキスト</param>
        /// <param name="execute">起動プログラムのファイルパス</param>
        private void SetButtonProperty(ref Button btn, string colorKey, string text, string execute)
        {
            var txtContainer = new StackPanel();
            txtContainer.Orientation = Orientation.Horizontal;
            // ■テキスト作成
            var txtMark = new TextBlock();
            txtMark.Text = "■";
            txtMark.Margin = new Thickness(0,0,2,0);

            // 色指定が有効なら■の色を変える
            if (ColorTable.ContainsKey(colorKey))
            {
                txtMark.Foreground = ColorTable[colorKey];
            }
            txtContainer.Children.Add(txtMark);

            // ボタン名テキスト作成
            var txt = new TextBlock();
            txt.Text = text;
            txtContainer.Children.Add(txt);

            // ボタンテキスト設定
            btn.Content = txtContainer;
            // ボタンクリック時の処理
            btn.Click += (sender, e) => 
            {
                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = execute;
                p.StartInfo.UseShellExecute = true;
                p.Start();
            };
        }

        /// <summary>
        /// 設定ファイルパス取得
        /// </summary>
        /// <returns>設定ファイルのフルパス。存在しない場合は空文字列を返す。</returns>
        private string GetConfigFilePath()
        {
            // EXEのパス取得
            string appPath = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            // 設定ファイルのフルパス組み立て
            string cfgFilePath = System.IO.Path.Combine(appPath, ConfigFileName);

            // 設定ファイル存在チェック
            if (System.IO.File.Exists(cfgFilePath))
            {
                return cfgFilePath;
            }
            else
            {
                // 設定ファイルなし
                return string.Empty;
            }
        }

    }
}