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


        private void OnbtnSetteiClicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingPage(), true);

        }

        private void OnSw_Kyuukei_Toggled(object sender, EventArgs e)
        {
            if (bIsCalculated == true) CalcDailyWage();

        }

        private void OnTpShukkin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (e.PropertyName == TimePicker.TimeProperty.PropertyName)//TimePickerにはTimeChangedイベントがないのでこのような処理が必要になる
            {
                Entry_Shukkin.Text = TpShukkin.Time.ToString("hh\':'mm");
                if (bIsCalculated == true) CalcDailyWage();
            }

        }

        private void OnTpTaisha_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TimePicker.TimeProperty.PropertyName)//TimePickerにはTimeChangedイベントがないのでこのような処理が必要になる
            {
                Entry_Taisha.Text = TpTaisha.Time.ToString("hh\':'mm");
                if (bIsCalculated == true) CalcDailyWage();
            }

        }


        private void OnEntry_Shukkin_TextChanged(object sender, EventArgs e)
        {
            if (Entry_Shukkin.Text == "")//エントリーを空にしたらTimePickerを12：00AMにして計算結果を消す
            {
                TpShukkin.SetValue(TimePicker.TimeProperty, TimeSpan.Zero);
                DeleteCalcResult();
                return;
            }
        }



        private void OnEntry_Taisha_TextChanged(object sender, EventArgs e)
        {
            if (Entry_Taisha.Text == "")//エントリーを空にしたらTimePickerを12：00AMにして計算結果を消す
            {
                TpTaisha.SetValue(TimePicker.TimeProperty, TimeSpan.Zero);
                DeleteCalcResult();
                return;
            }
        }

        private void OnEntry_Shukkin_Completed(object sender, EventArgs e)
        {
            Entry_Shukkin_UnfocusedOrCompleted();
        }
        private void OnEntry_Shukkin_Unfocused(object sender, EventArgs e)//時間入力が無効な値ならTimePickerから値を持ってくる
        {
            Entry_Shukkin_UnfocusedOrCompleted();
        }


        private void OnEntry_Taisha_Completed(object sender, EventArgs e)
        {
            Entry_Taisha_UnfocusedOrCompleted();
        }

        private void OnEntry_Taisha_Unfocused(object sender, EventArgs e)//時間入力が無効な値ならTimePickerから値を持ってくる
        {
            Entry_Taisha_UnfocusedOrCompleted();
        }

        private void Entry_Shukkin_UnfocusedOrCompleted()//Entry_Shukkinからフォーカスが離れた時、Enterが押された時の処理
        {
            TimeSpan ts;

            //出勤時間に入力されたstringの値がTimeSpan型に変換できるか
            if ((TimeSpan.TryParseExact(Entry_Shukkin.Text, "h':'m", null,//勉強メモ　TimeSpan型はDateTime型と違い:をシングルクオーテーションで囲む必要があります
                System.Globalization.TimeSpanStyles.None, out ts)) ||
                (TimeSpan.TryParseExact(Entry_Shukkin.Text, "hhmm", null,
                System.Globalization.TimeSpanStyles.None, out ts)))
            {//変換出来たら
                TpShukkin.SetValue(TimePicker.TimeProperty, ts);
            }

            Entry_Shukkin.Text = TpShukkin.Time.ToString("hh\':'mm");
        }

        private void Entry_Taisha_UnfocusedOrCompleted()//Entry_Taishaからフォーカスが離れた時、Enterが押された時の処理
        {
            TimeSpan ts;
            //退社時間に入力されたstringの値がTimeSpan型に変換できるか
            if ((TimeSpan.TryParseExact(Entry_Taisha.Text, "h':'m", null,//勉強メモ　TimeSpan型はDateTime型と違い:をシングルクオーテーションで囲む必要があります
                System.Globalization.TimeSpanStyles.None, out ts)) ||
                (TimeSpan.TryParseExact(Entry_Taisha.Text, "hhmm", null,
                System.Globalization.TimeSpanStyles.None, out ts)))
            {//変換出来たら
                TpTaisha.SetValue(TimePicker.TimeProperty, ts);
            }

            Entry_Taisha.Text = TpTaisha.Time.ToString("hh\':'mm");

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
        private void CalcDailyWage()//日給計算して表示
        {
            //Label_Message.Text = Entry_Shukkin.Text + Entry_Taisha.Text;

            //時間の計算をするためにTimePickerの値をDateTimeに変換する
            DateTime dt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                TpShukkin.Time.Hours, TpShukkin.Time.Minutes, 0);
            DateTime dt2 = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                TpTaisha.Time.Hours, TpTaisha.Time.Minutes, 0);

            TimeSpan tsTotal;

            //出勤から退社までの時間の計算 結果をtsTotalに格納
            if (dt2.CompareTo(dt) > 0)
            {//日をまたがない
                tsTotal = (dt2 - dt);
            }
            else
            {//日またぎ
                tsTotal = (dt - dt2);
                tsTotal = TimeSpan.FromHours(24) - tsTotal;
            }
            //表示
            Label_ResultDetail.Text = "出勤から退社:"
                + tsTotal.Hours + "時間" + tsTotal.Minutes + "分" + System.Environment.NewLine;

            Label_Message.Text = "日給はこの通りです";


            //深夜労働時間とそうでない労働時間の計算
            TimeSpan tsNight = CalcWorkingTime.NightWork(dt, dt2);//深夜手当がつく労働時間
            TimeSpan tsNormal = tsTotal - tsNight;//昼間(深夜でない)の労働時間


            int kyuukei = 0;
            if (Sw_Kyuukei.IsToggled)//休憩時間を引くトグルスイッチがオンなら休憩時間をひく
            {
                kyuukei = CalcWorkingTime.SubBreakTime(ref tsNormal, ref tsNight);//tsNormal,tsNightから休憩時間が引かれる
                Label_ResultDetail.Text += "休憩時間:" + kyuukei + "分" + System.Environment.NewLine;//さし引いた休憩時間を表示
            }
            else
            {
                Label_ResultDetail.Text += System.Environment.NewLine;
            }

            tsTotal = tsNormal + tsNight;//深夜労働＋そうでない労働＝合計労働時間

            //労働時間を表示
            Label_ResultDetail.Text += "労働時間:" + tsTotal.Hours + "時間" + tsTotal.Minutes + "分" + System.Environment.NewLine;

            if (tsNight.TotalMinutes > 0)//深夜労働がある場合は深夜労働時間を表示
            {
                Label_ResultDetail.Text += "うち深夜労働:" + tsNight.Hours + "時間" + tsNight.Minutes + "分" + System.Environment.NewLine;
            }
            else
            {
                Label_ResultDetail.Text += System.Environment.NewLine;
            }

            //給料を表示
            Label_ResultDailyWages.Text = " 給料：￥" +
                    String.Format("{0:#,0}", (
                    (tsNormal.Hours * jikyuu + tsNormal.Minutes * jikyuu / 60) +
                    (tsNight.Hours * jikyuu * 1.25 + tsNight.Minutes * jikyuu / 60 * 1.25)
                    )
                    ) + ".-";

            //給料がどのように計算されたかを表示
            Label_ResultDetail.Text += tsNormal.Hours + "時間" + tsNormal.Minutes + "分" + " * " + jikyuu + System.Environment.NewLine;
            if (tsNight.TotalMinutes > 0)
            {
                Label_ResultDetail.Text += tsNight.Hours + "時間" + tsNight.Minutes + "分" + " * " + jikyuu * 1.25;
            }

            bIsCalculated = true;//給料がすでに計算されていることを示すフラグ
        }


        private void DeleteCalcResult()//計算結果を消す
        {
            Label_Message.Text = "出社時間、退社時間を入力してください";
            Label_ResultDailyWages.Text = "";
            Label_ResultDetail.Text = "";
            bIsCalculated = false;
        }


        private void EnterCorrectTime()
        {
            Label_Message.Text = "適切な時刻を入力してください";
            Label_ResultDailyWages.Text = "";
            bIsCalculated = false;
        }

        //private void SaveSettings()
        //{
        //    container.Values["Entry_Shukkin.Text"] = Entry_Shukkin.Text;
        //    container.Values["Entry_Taisha.Text"] = Entry_Taisha.Text;
        //    container.Values["ToSw_Kyuukei.IsOn"] = ToSw_Kyuukei.IsOn.ToString();
        //    container.Values["Label_Message.Text"] = Label_Message.Text;
        //    container.Values["Label_ResultTS.Text"] = Label_ResultTS.Text;
        //    container.Values["Label_Label_ResultDailyWages.Text"] = Label_Label_ResultDailyWages.Text;
        //    container.Values["bIsCalculated"] = bIsCalculated.ToString();
        //}

        //private void LoadSettings()
        //{
        //    Entry_Shukkin.Text = container.Values["Entry_Shukkin.Text"].ToString();
        //    Entry_Taisha.Text = container.Values["Entry_Taisha.Text"].ToString();
        //    ToSw_Kyuukei.IsOn = (container.Values["ToSw_Kyuukei.IsOn"].ToString() == "True");
        //    Label_Message.Text = container.Values["Label_Message.Text"].ToString();
        //    Label_ResultTS.Text = container.Values["Label_ResultTS.Text"].ToString();
        //    Label_Label_ResultDailyWages.Text = container.Values["Label_Label_ResultDailyWages.Text"].ToString();
        //    bIsCalculated = (container.Values["bIsCalculated"].ToString() == "True");
        //}
    }
}
