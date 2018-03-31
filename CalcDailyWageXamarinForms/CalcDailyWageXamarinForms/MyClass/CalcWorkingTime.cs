using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




using System.Diagnostics;//assert用

namespace CalcDailyWageXamarinForms.ToLibrary
{
    /// <summary>
    /// ２つのDateTime型、もしくはひとつのTimeSpan型から労働時間、深夜労働時間、
    /// 休憩時間などの計算を行う機能を提供するクラス。
    /// このクラスの出社時間、退社時間を表すDateTime型の引数は、時間、分の部分のみを使用することを
    /// 前提としており、呼び出し元が引数を渡す際に、年月日に関しては、今日の日付、秒に関しては
    /// 00を設定しておくことを前提に
    /// </summary>
    public static class CalcWorkingTime
    {

        /// <summary>
        /// 労働時間を表すTimeSpan型のオブジェクトから、休憩時間を引く。通常労働時間から優先してひく。
        /// 休憩時間のルール　:　出社から退社が５時間超の場合３０分、１０時間超の場合１時間
        /// </summary>
        /// <param name="tsNormal">通常労働時間（深夜でない）</param>
        /// <param name="tsNight">深夜労働時間</param>
        /// <returns>差し引いた分数(Minuites)</returns>
        public static int SubBreakTime(ref TimeSpan tsNormal, ref TimeSpan tsNight)
        {
            TimeSpan tsTotal = tsNormal + tsNight;
            TimeSpan tsToCompare;
            int minSub;//差し引く時間
            if ((tsNormal.TotalMinutes < 0) || (tsNight.TotalMinutes < 0))
            {
                throw new Exception("SubBreakTimeの引数が無効な値です");
            }

            tsToCompare = new TimeSpan(5, 0, 0);
            if (tsTotal < tsToCompare) return 0;//5時間未のとき：引かない。

            tsToCompare = new TimeSpan(5, 30, 0);
            if (tsTotal < tsToCompare)//5時間以上５時間半未満のとき：５時間分を超えた分ひく
            {
                minSub = tsTotal.Minutes;
                SubPrior(minSub, ref tsNormal, ref tsNight);
                return minSub;
            }
            tsToCompare = new TimeSpan(10, 00, 0);
            if (tsTotal < tsToCompare)//５時間半以上１０時間未満のとき：３０分ひく
            {
                minSub = 30;
                SubPrior(minSub, ref tsNormal, ref tsNight);
                return minSub;
            }
            tsToCompare = new TimeSpan(10, 30, 0);
            if (tsTotal < tsToCompare)//10時間以上１０時間30分未満のとき：３０分+１０時間を超えた分だけひく
            {
                minSub = 30 + tsTotal.Minutes;
                SubPrior(minSub, ref tsNormal, ref tsNight);
                return minSub;
            }
            //１０時間半以上のとき：１時間ひく
            minSub = 60;
            SubPrior(minSub, ref tsNormal, ref tsNight);
            return minSub;
        }


        //出社時間と終業時間から深夜労働した時間を割り出す
        //戻り値:深夜労働した時間
        static public TimeSpan NightWork(DateTime dtStart, DateTime dtLeave)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0);
            DateTime fiveAM = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                5, 0, 0);
            DateTime TenPM = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                22, 0, 0);

            if (dtLeave.CompareTo(dtStart) > 0)
            {//労働が日をまたがない
                if ((dtStart.CompareTo(TenPM) >= 0)
                    || (dtLeave.CompareTo(fiveAM) <= 0))
                {//出勤時間が22時以降、または終業時間が朝5時以前のとき：全て深夜労働
                    return dtLeave - dtStart;
                }

                if (dtStart.CompareTo(fiveAM) < 0)
                {//出勤時間が朝5時よりも前なとき：朝5時以前を深夜労働時間に追加
                    ts = ts.Add(fiveAM - dtStart);
                }
                if (dtLeave.CompareTo(TenPM) > 0)
                {//終業時間が22時よりも後のとき：22時以降を深夜労働時間に追加
                    ts = ts.Add(dtLeave - TenPM);
                }
                return ts;
            }
            else
            {//労働が日をまたぐ

                TimeSpan twoHours = TimeSpan.FromHours(2);
                TimeSpan fiveHours = TimeSpan.FromHours(5);

                if (dtStart.CompareTo(fiveAM) < 0)
                {//出勤時間が朝5時よりも前なとき：朝5時以前と22時～24時を深夜労働時間に追加
                    ts = ts.Add(fiveAM - dtStart);
                    ts = ts.Add(twoHours);
                }
                else if (dtStart.CompareTo(TenPM) <= 0)
                {//出勤時間が朝5時以降、22時以前のとき：22時～24時を深夜労働時間に追加
                    ts = ts.Add(twoHours);
                }
                else
                {//出勤時間が22時よりも後のとき：出勤時間～24時を深夜労働時間に追加
                    DateTime twelveAM = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                        0, 0, 0);
                    twelveAM = twelveAM.AddDays(1);
                    ts = ts.Add(twelveAM - dtStart);



                }


                if (dtLeave.CompareTo(fiveAM) < 0)
                {//終業時間が朝5時よりも前なとき：0時～終業時間を深夜労働時間に追加
                    DateTime zeroAM = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                        0, 0, 0);
                    ts = ts.Add(dtLeave - zeroAM);
                }
                else if (dtLeave.CompareTo(TenPM) <= 0)
                {//終業時間が朝5時以降、22時以前のとき：0時～5時を深夜労働時間に追加
                    ts = ts.Add(fiveHours);
                }
                else
                {//終業時間が22時よりも後のとき：0時～5時と22時～終業時間を深夜労働時間に追加
                    ts = ts.Add(fiveHours);
                    ts = ts.Add(dtLeave - TenPM);
                }

                return ts;
            }

        }//NightWorkメソッド


        /// <summary>
        /// TimeSpan型のオブジェクトから、第一引数で指定した分(Minutes)をさしひく
        /// まずts1から引いて、余ったら残りをts2からひく
        /// </summary>
        /// <param name="iSubMin">差し引く分数(minutes)</param>
        /// <param name="ts1">優先して引かれる</param>
        /// <param name="ts2">2番目に優先して引かれる</param>
        private static void SubPrior(int iSubMin, ref TimeSpan ts1, ref TimeSpan ts2)
        {
            Debug.Assert((ts1.TotalMinutes + ts2.TotalMinutes) >= iSubMin);//引き切れる

            TimeSpan tsSub = TimeSpan.FromMinutes(iSubMin);

            if (tsSub <= ts1)
            {//ts1から全部引ける時　
                ts1 = ts1 - tsSub; return;
            }
            else
            {//ts1から引いても余る時
                tsSub = tsSub - ts1;//余りを
                ts2 = ts2 - tsSub;//ts2から引く
                ts1 = TimeSpan.Zero;//ts1はゼロ
            }
        }


    }//CalcWorkingTimeクラス







}
