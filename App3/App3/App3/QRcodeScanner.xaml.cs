using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace App3
{
    public partial class QRcodeScanner : ContentPage
    {
        public QRcodeScanner()
        {
            InitializeComponent();
        }

        // 存QR掃到的資料
        string QRstring;

        // 建立
        public class QRInfo
        {
            public string Uid { get; set; }
            public string QRresult { get; set; }
            public string Time { get; set; }
        }
        // 接收回PHP傳值的類別
        public class PHPresult
        {
            public string result { get; set; }
        }

        public async void Scan()
        {
            // 建立一個掃描page
            var scan = new ZXingScannerPage();
            // 以非同步方法將page新增到堆疊頂端
            await Navigation.PushAsync(scan);
            scan.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    // 掃到後將page移除
                    await Navigation.PopAsync();
                    QRstring = result.Text;
                });
            };
            // 如果有掃到
            if(QRstring != null)
            {
                //建立物件
                QRInfo qRInfo = new QRInfo
                {
                    Uid = App3.session.DisplayUserId,
                    QRresult = QRstring,
                    Time = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
                };

                // 物件序列化
                string strJson = JsonConvert.SerializeObject(qRInfo, Formatting.Indented);
                string strJson2 = "[" + strJson + "]";
                Console.WriteLine(strJson2);
                ToPHP(strJson2);
            }
        }

        // HttpClient
        public async void ToPHP(string Str)
        {
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    try
                    {
                        // 目標php檔
                        string FooUrl = $"https://qrcodeapi.000webhostapp.com/qrcode.php";
                        HttpResponseMessage response = null;

                        //設定相關網址內容
                        var fooFullUrl = $"{FooUrl}";

                        // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
                        client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        // Content-Type 用於宣告遞送給對方的文件型態
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        using (var fooContent = new StringContent(Str, Encoding.UTF8, "application/json"))
                        {
                            response = await client.PostAsync(fooFullUrl, fooContent);
                        }
                        Console.WriteLine("response = " + response);
                        // PHP回傳值
                        string strResult = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("strResult = " + strResult);
                        // 反序列化
                        PHPresult yesORno = JsonConvert.DeserializeObject<PHPresult>(strResult);

                        Console.WriteLine("result = " + yesORno.result);
                        if (yesORno.result == "success")
                        {
                            Console.WriteLine("update sucess!");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("update fail");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }
    }
}
