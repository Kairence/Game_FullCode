using System;
using Server;
using Server.Mobiles;
using Server.Items;
using System.Collections.Generic;
using System.Linq;

namespace Server.Misc
{
    public static class SetItem
    {
        #region 세트 옵션 데이터 (GetSetData)
        
        // ReadOnlySpan[] 배열 대신 int[][] (가변 배열)을 사용하여 오류 해결
        public static int[][] GetSetData(int setID) => setID switch
        {
            1 => [
                [114, 100000, 4, 1000000],
                [0, 5000, 1, 5000, 2, 5000],
                [19, 20000, 20, 20000, 21, 20000, 117, 100000],
                [118, 100000],
                [118, 100000, 114, 150000]
            ],
            2 => [
                [4, 1000000, 5, 2000000, 40, 50000],
                [4, 1500000, 5, 2500000, 7, 200000],
                [114, 100000, 40, 150000, 77, 50000]
            ],
            3 => [
                [4, 3000000, 19, 15000],
                [12, 300000, 114, 100000],
                [4, 10500000]
            ],
            4 => [
                [114, 50000, 19, 5000, 20, 5000, 21, 5000],
                [117, 100000, 19, 5000, 20, 5000, 21, 5000],
                [114, 150000],
                [4, 1000000, 5, 1000000, 6, 1000000],
                [118, 100000, 19, 10000, 20, 10000, 21, 10000]
            ],
            5 => [
                [114, 100000, 12, 100000],
                [118, 100000, 117, 100000],
                [17, 150000, 8, 150000],
                [119, 100000, 115, 100000],
                [19, 30000, 20, 30000, 21, 30000]
            ],
            6 => [
                [3, 2000000, 114, 100000],
                [117, 100000],
                [3, 3000000, 118, 50000],
                [42, 30000, 43, 30000],
                [3, 5000000, 44, 100000, 45, 100000]
            ],
            7 => [
                [14, 150000, 16, 150000],
                [1, 4000, 2, 4000, 17, 100000],
                [114, 100000, 104, 100000],
                [40, 100000, 20, 30000, 21, 40000]
            ],
            8 => [
                [124, 200, 13, 200000],
                [118, 100000],
                [124, 300, 117, 100000],
                [134, 150000, 17, 100000, 21, 50000]
            ],
            9 => [
                [125, 200, 14, 200000],
                [118, 100000],
                [125, 300, 117, 100000],
                [135, 150000, 17, 100000, 21, 50000]
            ],
            10 => [
                [126, 200, 15, 200000],
                [118, 100000],
                [126, 300, 117, 100000],
                [136, 150000, 17, 100000, 21, 50000]
            ],
            11 => [
                [127, 200, 16, 200000],
                [118, 100000],
                [127, 300, 117, 100000],
                [137, 150000, 17, 100000, 21, 50000]
            ],
            12 => [
                [128, 100, 114, 200000],
                [118, 100000],
                [128, 150, 117, 100000],
                [138, 75000, 17, 100000, 21, 50000]
            ],
            13 => [
                [129, 100, 114, 200000],
                [118, 100000],
                [129, 150, 117, 100000],
                [139, 75000, 17, 100000, 21, 50000]
            ],
            14 => [
                [4, 5000000, 114, 200000],
                [118, 150000, 117, 150000],
                [1, 50000, 2, 50000],
                [42, 100000, 43, 100000, 18, 100000]
            ],
            15 => [
                [5, 1000000, 114, 50000],
                [118, 50000],
                [1, 15000],
                [117, 100000, 17, 100000]
            ],
            16 => [
                [0, 20000, 12, 150000, 117, 100000],
                [118, 100000, 42, 50000]
            ],
            17 => [
                [4, 3000000, 12, 150000],
                [12, 150000, 114, 100000],
                [4, 5000000, 114, 100000],
                [18, 150000, 114, 100000],
                [17, 150000, 19, 50000, 117, 100000]
            ],
            18 => [
                [19, 10000, 20, 10000, 21, 10000, 114, 100000],
                [117, 150000, 114, 100000],
                [19, 10000, 20, 10000, 21, 10000, 118, 150000],
                [1, 40000, 2, 40000],
                [19, 10000, 20, 10000, 21, 10000, 117, 150000]
            ],
            19 => [
                [40, 100000, 7, 150000],
                [7, 200000, 5, 2000000],
                [42, 100000, 17, 150000, 18, 100000]
            ],
            20 => [
                [110, 100000, 8, 150000],
                [8, 200000, 6, 2000000],
                [43, 100000, 119, 100000, 45, 100000]
            ],
            _ => Array.Empty<int[]>()
        };

        #endregion

        public static void SetOption(PlayerMobile pm, bool reload = true)
        {
            if (pm == null) return;

            // 1. 카운트 초기화 및 갱신
            if (reload)
            {
                Array.Clear(pm.ItemSetValue, 0, pm.ItemSetValue.Length);
                foreach (var item in pm.Items.OfType<IEquipOption>())
                {
                    if (item.PrefixOption[50] is var id && id > 0)
                    {
                        if (id < pm.ItemSetValue.Length)
                            pm.ItemSetValue[id]++;
                    }
                }
            }

            // 2. 누적 옵션 배열 초기화
            Array.Clear(pm.ItemSetSaveValue, 0, pm.ItemSetSaveValue.Length);

            // 3. 고성능 옵션 합산 루프
            for (int i = 1; i < pm.ItemSetValue.Length; i++)
            {
                int equippedCount = pm.ItemSetValue[i];
                if (equippedCount < 2) continue;

                int[][] setSteps = GetSetData(i);
                
                // 장착 개수에 맞는 단계까지 루프 (2파츠 장착 시 stepIdx 0만 실행)
                for (int stepIdx = 0; stepIdx < setSteps.Length && stepIdx <= (equippedCount - 2); stepIdx++)
                {
                    int[] currentStep = setSteps[stepIdx];
                    if (currentStep == null || currentStep.Length == 0) continue;

                    // 실제 옵션 데이터 합산 (Index 0부터 ID, Value 쌍으로 바로 접근)
                    // 기존 코드의 '옵션개수 헤더'를 제거하고 바로 데이터로 접근하도록 최적화
                    for (int k = 0; k < currentStep.Length; k += 2)
                    {
                        if (k + 1 >= currentStep.Length) break;

                        int optionID = currentStep[k];
                        if (optionID >= 0 && optionID < pm.ItemSetSaveValue.Length)
                        {
                            pm.ItemSetSaveValue[optionID] += currentStep[k + 1];
                        }
                    }
                }
            }

            // 4. 모든 장비 툴팁 갱신 트리거
            foreach (var item in pm.Items.OfType<IEquipOption>())
            {
                if (item.PrefixOption[50] > 0 && item is Item actualItem)
                {
                    actualItem.InvalidateProperties();
                }
            }

            // 5. 모바일 상태 업데이트 동기화
            pm.ComputeResistances();
            pm.Delta(MobileDelta.Stat);
            pm.CheckStatTimers();
            pm.ProcessDelta();
        }
    }
}
