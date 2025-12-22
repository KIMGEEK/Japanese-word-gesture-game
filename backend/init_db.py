# init_db.py
from main import SessionLocal, Word, Base, engine
import json
import random

# ✅ 필요하면 초기화 켜기 (DB 구조 바뀐 경우 권장)
Base.metadata.drop_all(bind=engine)
Base.metadata.create_all(bind=engine)

HIRAGANA_POOL = list("あいうえおかきくけこさしすせそたちつてとなにぬねの"
                     "はひふへほまみむめもやゆよらりるれろわをん")

def is_hiragana_only(s: str) -> bool:
    return all('\u3040' <= ch <= '\u309F' for ch in s)

def make_candidates(word: str) -> list[str]:
    """
    후보군은 '히라가나 글자 5개' 고정.
    - 정답 단어에 들어있는 글자들을 우선 포함
    - 글자 수가 5 미만이면, 단어에 없는 글자를 랜덤으로 추가
    - 최종 5개로 셔플
    """
    chars = list(dict.fromkeys(word))  # 중복 제거(순서 유지)
    # 단어가 5글자인데 중복이 많으면 chars가 5 미만일 수 있음 -> 아래에서 채움
    while len(chars) < 5:
        pick = random.choice(HIRAGANA_POOL)
        if pick not in chars:
            chars.append(pick)
    # 5개 초과면 5개로 자르기(원칙상 단어가 2~5이므로 보통 초과 없음)
    chars = chars[:5]
    random.shuffle(chars)
    return chars

def init_db():
    db = SessionLocal()

    # ✅ 전부 "히라가나 2~5글자" + 기초 어휘(사물/과일/색/인칭 등)
    words_data = [
        # Level 1 (아주 기초)
        {"japanese": "りんご", "korean": "사과", "level": 1},
        {"japanese": "みかん", "korean": "귤", "level": 1},
        {"japanese": "ぶどう", "korean": "포도", "level": 1},
        {"japanese": "もも", "korean": "복숭아", "level": 1},
        {"japanese": "ばなな", "korean": "바나나", "level": 1},
        {"japanese": "みず", "korean": "물", "level": 1},
        {"japanese": "おちゃ", "korean": "차", "level": 1},
        {"japanese": "ぱん", "korean": "빵", "level": 1},
        {"japanese": "ごはん", "korean": "밥", "level": 1},
        {"japanese": "いえ", "korean": "집", "level": 1},
        {"japanese": "へや", "korean": "방", "level": 1},
        {"japanese": "ほん", "korean": "책", "level": 1},
        {"japanese": "ぺん", "korean": "펜", "level": 1},
        {"japanese": "かばん", "korean": "가방", "level": 1},
        {"japanese": "くつ", "korean": "신발", "level": 1},
        {"japanese": "そら", "korean": "하늘", "level": 1},
        {"japanese": "うみ", "korean": "바다", "level": 1},
        {"japanese": "やま", "korean": "산", "level": 1},
        {"japanese": "はな", "korean": "꽃", "level": 1},

        # Level 2 (자주 쓰는 기본)
        {"japanese": "くるま", "korean": "자동차", "level": 2},
        {"japanese": "でんしゃ", "korean": "전철", "level": 2},
        {"japanese": "じてんしゃ", "korean": "자전거", "level": 2},  # 4글자
        {"japanese": "とけい", "korean": "시계", "level": 2},
        {"japanese": "てがみ", "korean": "편지", "level": 2},
        {"japanese": "でんわ", "korean": "전화", "level": 2},
        {"japanese": "かさ", "korean": "우산", "level": 2},
        {"japanese": "まど", "korean": "창문", "level": 2},
        {"japanese": "いす", "korean": "의자", "level": 2},
        {"japanese": "つくえ", "korean": "책상", "level": 2},
        {"japanese": "せんせい", "korean": "선생님", "level": 2},
        {"japanese": "がくせい", "korean": "학생", "level": 2},
        {"japanese": "ともだち", "korean": "친구", "level": 2},
        {"japanese": "かぞく", "korean": "가족", "level": 2},
        {"japanese": "わたし", "korean": "나(저)", "level": 2},
        {"japanese": "あなた", "korean": "당신", "level": 2},
        {"japanese": "かれ", "korean": "그(남)", "level": 2},
        {"japanese": "かのじょ", "korean": "그녀", "level": 2},  # 4글자

        # Level 3 (기초 안에서 조금 응용/추상)
        {"japanese": "しろい", "korean": "하얗다", "level": 3},
        {"japanese": "くろい", "korean": "검다", "level": 3},
        {"japanese": "あかい", "korean": "빨갛다", "level": 3},
        {"japanese": "あおい", "korean": "파랗다", "level": 3},
        {"japanese": "きいろ", "korean": "노랑", "level": 3},
        {"japanese": "みどり", "korean": "초록", "level": 3},
        {"japanese": "むらさき", "korean": "보라", "level": 3},  # 4글자
        {"japanese": "あさごはん", "korean": "아침밥(아침식사)", "level": 3},  # 5글자
        {"japanese": "やくそく", "korean": "약속", "level": 3},
        {"japanese": "しごと", "korean": "일", "level": 3},
    ]

    try:
        print("데이터 추가 중...")

        for item in words_data:
            jp = item["japanese"]

            # ✅ 조건 검증
            if not (2 <= len(jp) <= 5):
                print(f"스킵(길이 조건 위반): {jp}")
                continue
            if not is_hiragana_only(jp):
                print(f"스킵(히라가나 아님): {jp}")
                continue

            candidates = make_candidates(jp)

            exists = db.query(Word).filter(Word.japanese == jp).first()
            if not exists:
                word = Word(
                    japanese=jp,
                    korean=item["korean"],
                    level=item["level"],
                    candidates=json.dumps(candidates, ensure_ascii=False)
                )
                db.add(word)
                print(f"추가됨: {jp} / 후보군: {candidates}")
            else:
                print(f"이미 있음: {jp}")

        db.commit()
        print("모든 데이터 추가 완료!")

    except Exception as e:
        print(f"에러 발생: {e}")
        db.rollback()
    finally:
        db.close()

if __name__ == "__main__":
    init_db()
