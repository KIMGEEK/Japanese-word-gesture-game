from main import SessionLocal, Word, Base, engine

# 1. 기존 테이블 비우고 새로 만들기 (초기화)
# Base.metadata.drop_all(bind=engine)
# Base.metadata.create_all(bind=engine)

def init_db():
    db = SessionLocal()

    # 2. 추가할 단어 리스트
    words_data = [
        {"japanese": "猫", "korean": "고양이", "level": 1},
        {"japanese": "犬", "korean": "개", "level": 1},
        {"japanese": "りんご", "korean": "사과", "level": 1},
        {"japanese": "学校", "korean": "학교", "level": 2},
        {"japanese": "先生", "korean": "선생님", "level": 2},
        {"japanese": "宇宙", "korean": "우주", "level": 3},
        {"japanese": "約束", "korean": "약속", "level": 3},
    ]

    try:
        # 3. 반복문으로 DB에 추가
        print("데이터 추가 중...")
        for item in words_data:
            # 이미 있는지 확인 (중복 방지 로직)
            exists = db.query(Word).filter(Word.japanese == item["japanese"]).first()
            if not exists:
                word = Word(japanese=item["japanese"], korean=item["korean"], level=item["level"])
                db.add(word)
                print(f"추가됨: {item['japanese']}")
            else:
                print(f"이미 있음: {item['japanese']}")
        
        db.commit()
        print("모든 데이터 추가 완료!")

    except Exception as e:
        print(f"에러 발생: {e}")
        db.rollback()
    finally:
        db.close()

if __name__ == "__main__":
    init_db()