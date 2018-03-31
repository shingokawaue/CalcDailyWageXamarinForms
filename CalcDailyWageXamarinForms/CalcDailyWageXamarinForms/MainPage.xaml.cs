using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using PCLStorage;
using CalcDailyWageXamarinForms.ToLibrary;

namespace CalcDailyWageXamarinForms
{
	public partial class MainPage : ContentPage
	{
        static int jikyuu = 1000;
        static bool bIsCalculated = false;

        //PCLStorage.IFolder container = PCLStorage.FileSystem.Current.LocalStorage;
        public MainPage()
		{
			InitializeComponent();

            //if (container.Values.ContainsKey("Jikyuu"))
            //{
            //    jikyuu = (int)container.Values["Jikyuu"];
            //}
            //else
            //{
            //    jikyuu = 1000;
            //    container.Values["Jikyuu"] = 1000;
            //}
            //txtblk_Jikyuu.Text = "時給 : " + jikyuu.ToString();
            //if (container.Values.ContainsKey("ToSw_Kyuukei.IsOn"))
            //{
            //    ToSw_Kyuukei.IsOn = (container.Values["ToSw_Kyuukei.IsOn"].ToString() == "True");
            //}

        }



        private void Button_Calc_Click(object sender, EventArgs e)
        {
            CalcDailyWage();
        }

        private void ToSw_Kyuukei_Toggled(object sender, EventArgs e)
        {
            if (bIsCalculated == true) CalcDailyWage();
        }

        private void OnbtnSetteiClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingPage(), true);

        }

        // ページ遷移直後にOnNavigatedToイベントハンドラーが呼び出される
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    // このようにe.Parameterで前のページから渡された値を取得できます。
        //    // 値はキャストして取り出します。
        //    string param = e.Parameter as string;
        //    if (param == "SettingPage")
        //    {
        //        jikyuu = (int)container.Values["Jikyuu"];
        //        LoadSettings();
        //    }
        //    base.OnNavigatedTo(e);


        //}


        //4. Suspendingイベントのイベントハンドラ
        //private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        //{
        //    SaveSettings();
        //}









        //----------------------------------------------------------------------------------------------------
        //                                  その他のメソッド↓
        //------------------------------------------------------------------------------------------
        private void CalcDailyWage()//日給計算
        {
            Label_Message.Text = Editor_Shukkin.Text + Editor_Taisha.Text;

            //DateTimeに変換できるか確かめる
            DateTime dt, dt2;

            //出勤時間に入力された値がDate型に変換できるか
            if (!(DateTime.TryParseExact(Editor_Shukkin.Text, "H:m", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt)) &&
                !(DateTime.TryParseExact(Editor_Shukkin.Text, "HHmm", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt)))
            {
                EnterCorrectTime(); return;//変換できなかったらリターン
            }

            //退社時間に入力された値がDate型に変換できるか
            if (!(DateTime.TryParseExact(Editor_Taisha.Text, "H:m", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt2)) &&
                 !(DateTime.TryParseExact(Editor_Taisha.Text, "HHmm", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dt2)))
            {
                EnterCorrectTime(); return;//変換できなかったらリターン
            }

            //変換出来たら、dt,dt2に値が入っているので給料計算
            TimeSpan tsTotal;


            if (dt2.CompareTo(dt) > 0)
            {//日をまたがない
                tsTotal = (dt2 - dt);
            }
            else
            {//日またぎ
                tsTotal = (dt - dt2);
                tsTotal = TimeSpan.FromHours(24) - tsTotal;
            }

            Label_ResultTS.Text = "出勤から退社:"
                + tsTotal.Hours + "時間" + tsTotal.Minutes + "分";

            Label_Message.Text = "日給はこの通りです";
            //給料の計算


            TimeSpan tsNight = CalcWorkingTime.NightWork(dt, dt2);//深夜手当がつく労働時間
            TimeSpan tsNormal = tsTotal - tsNight;//昼間(深夜でない)の労働時間


            int kyuukei = 0;
            if (Sw_Kyuukei.IsToggled)//休憩時間を引くトグルスイッチがオンなら
            {
                kyuukei = CalcWorkingTime.SubBreakTime(ref tsNormal, ref tsNight);//tsNormal,tsNightから休憩時間が引かれる
                Label_ResultTS.Text = Label_ResultTS.Text + System.Environment.NewLine 
                    + "休憩時間:" + kyuukei + "分";//さし引いた休憩時間を表示
            }

            tsTotal = tsNormal + tsNight;
            Label_ResultTS.Text = Label_ResultTS.Text + System.Environment.NewLine  + 
                "労働時間:" + tsTotal.Hours + "時間" + tsTotal.Minutes + "分";//労働時間表示

            if (tsNight.TotalMinutes > 0)
            {
                Label_ResultTS.Text = Label_ResultTS.Text + System.Environment.NewLine 
                    + "うち深夜労働:" + tsNight.Hours + "時間" + tsNight.Minutes + "分";
            }

            Label_ResultMoney.Text = " 給料：￥" +
                    String.Format("{0:#,0}", (
                    (tsNormal.Hours * jikyuu + tsNormal.Minutes * jikyuu / 60) +
                    (tsNight.Hours * jikyuu * 1.25 + tsNight.Minutes * jikyuu / 60 * 1.25)
                    )
                    ) + ".-";

            Label_ResultDetail.Text = tsNormal.Hours + "時間" + tsNormal.Minutes + "分" + " * " + jikyuu;
            if (tsNight.TotalMinutes > 0)
            {
                Label_ResultDetail.Text += "　　" + tsNight.Hours + "時間" + tsNight.Minutes + "分" + " * " + jikyuu * 1.25;
            }


            bIsCalculated = true;



        }





        private void EnterCorrectTime()
        {
            Label_Message.Text = "適切な時刻を入力してください";
            Label_ResultMoney.Text = "";
            Label_ResultTS.Text = "";
        }

        //private void SaveSettings()
        //{
        //    container.Values["Editor_Shukkin.Text"] = Editor_Shukkin.Text;
        //    container.Values["Editor_Taisha.Text"] = Editor_Taisha.Text;
        //    container.Values["ToSw_Kyuukei.IsOn"] = ToSw_Kyuukei.IsOn.ToString();
        //    container.Values["Label_Message.Text"] = Label_Message.Text;
        //    container.Values["Label_ResultTS.Text"] = Label_ResultTS.Text;
        //    container.Values["Label_ResultMoney.Text"] = Label_ResultMoney.Text;
        //    container.Values["bIsCalculated"] = bIsCalculated.ToString();
        //}

        //private void LoadSettings()
        //{
        //    Editor_Shukkin.Text = container.Values["Editor_Shukkin.Text"].ToString();
        //    Editor_Taisha.Text = container.Values["Editor_Taisha.Text"].ToString();
        //    ToSw_Kyuukei.IsOn = (container.Values["ToSw_Kyuukei.IsOn"].ToString() == "True");
        //    Label_Message.Text = container.Values["Label_Message.Text"].ToString();
        //    Label_ResultTS.Text = container.Values["Label_ResultTS.Text"].ToString();
        //    Label_ResultMoney.Text = container.Values["Label_ResultMoney.Text"].ToString();
        //    bIsCalculated = (container.Values["bIsCalculated"].ToString() == "True");
        //}
    }
}
